import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useRef,
  useState,
  type ReactNode,
} from 'react';
import { authStorage } from '../services/authStorage';
import * as authService from '../services/authService';
import type { AuthenticatedUser, LoginRequest, LoginResponse } from '../types/auth';
import { decodeJwt, type JwtPayload } from '../utils/jwt';

interface AuthContextValue {
  accessToken: string | null;
  refreshToken: string | null;
  accessTokenExpiresOnUtc: string | null;
  refreshTokenExpiresOnUtc: string | null;
  user: AuthenticatedUser | null;
  isAuthenticated: boolean;
  isAuthenticating: boolean;
  isHydrating: boolean;
  login: (credentials: LoginRequest) => Promise<LoginResponse>;
  logout: () => void;
  refresh: () => Promise<void>;
}

const REFRESH_BUFFER_MS = 60_000;

const EMAIL_CLAIMS = ['email', 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'];
const NAME_CLAIMS = ['name', 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'];
const ROLE_CLAIMS = ['role', 'roles', 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
const USER_ID_CLAIMS = ['sub', 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'];
const TEAM_ID_CLAIMS = ['teamId'];

const getClaimValue = <T,>(payload: JwtPayload, keys: string[]): T | undefined => {
  for (const key of keys) {
    const value = payload[key];
    if (value !== undefined && value !== null) {
      return value as T;
    }
  }
  return undefined;
};

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

export const AuthProvider = ({ children }: { children: ReactNode }) => {
  const [accessToken, setAccessToken] = useState<string | null>(null);
  const [refreshToken, setRefreshToken] = useState<string | null>(null);
  const [accessTokenExpiresOnUtc, setAccessTokenExpiresOnUtc] = useState<string | null>(null);
  const [refreshTokenExpiresOnUtc, setRefreshTokenExpiresOnUtc] = useState<string | null>(null);
  const [user, setUser] = useState<AuthenticatedUser | null>(null);
  const [isAuthenticating, setIsAuthenticating] = useState(false);
  const [isHydrating, setIsHydrating] = useState(true);

  const refreshTimeoutRef = useRef<number | null>(null);
  const isRefreshingRef = useRef(false);
  const refreshSessionRef = useRef<(() => Promise<void>) | null>(null);

  const clearRefreshTimer = useCallback(() => {
    if (refreshTimeoutRef.current !== null) {
      clearTimeout(refreshTimeoutRef.current);
      refreshTimeoutRef.current = null;
    }
  }, []);

  const logout = useCallback(() => {
    clearRefreshTimer();
    authStorage.clear();
    setAccessToken(null);
    setRefreshToken(null);
    setAccessTokenExpiresOnUtc(null);
    setRefreshTokenExpiresOnUtc(null);
    setUser(null);
    setIsHydrating(false);
  }, [clearRefreshTimer]);

  const applyClaimsFromToken = useCallback(
    (token: string): string | null => {
      const claims = decodeJwt(token);
      if (!claims) {
        return null;
      }

      const userId = getClaimValue<string>(claims, USER_ID_CLAIMS);
      if (!userId) {
        return null;
      }

      const email = getClaimValue<string>(claims, EMAIL_CLAIMS);
      const fullName = getClaimValue<string>(claims, NAME_CLAIMS) ?? email ?? userId;
      const roleClaim = getClaimValue<string | string[]>(claims, ROLE_CLAIMS);
      const roleName = Array.isArray(roleClaim) ? roleClaim[0]?.toString() : roleClaim?.toString();
  const teamIdClaim = getClaimValue<string>(claims, TEAM_ID_CLAIMS) ?? null;
  const expiresIso = typeof claims.exp === 'number' ? new Date(claims.exp * 1000).toISOString() : null;

  setAccessTokenExpiresOnUtc(expiresIso);

      setUser((prev) => {
        const base: AuthenticatedUser =
          prev ?? ({
            userId,
            email: email ?? '',
            fullName,
            teamId: teamIdClaim ?? null,
          } as AuthenticatedUser);

        return {
          ...base,
          userId,
          email: email ?? base.email,
          fullName: fullName ?? base.fullName,
          roleName: roleName ?? base.roleName,
          teamId: teamIdClaim ?? base.teamId ?? null,
          accessTokenExpiresOnUtc: expiresIso ?? base.accessTokenExpiresOnUtc,
        };
      });

      return expiresIso;
    },
    [],
  );

  const scheduleRefresh = useCallback(
    (expiresIso?: string | null) => {
      clearRefreshTimer();
      if (!expiresIso) {
        return;
      }

      const expiresAt = Date.parse(expiresIso);
      if (Number.isNaN(expiresAt)) {
        return;
      }

      const delay = Math.max(expiresAt - Date.now() - REFRESH_BUFFER_MS, 5_000);
      refreshTimeoutRef.current = setTimeout(() => {
        refreshSessionRef.current?.().catch((error) => {
          console.error('Automatic token refresh failed', error);
          logout();
        });
      }, delay);
    },
    [clearRefreshTimer, logout],
  );

  const hydrateProfile = useCallback(async () => {
    try {
      const result = await authService.getProfile();
      const profile = result.data;

      setUser((prev) => {
        const base: AuthenticatedUser =
          prev ?? ({
            userId: profile.id,
            email: profile.email,
            fullName: profile.fullName,
            teamId: profile.teamId ?? null,
          } as AuthenticatedUser);

        return {
          ...base,
          userId: profile.id,
          email: profile.email,
          fullName: profile.fullName,
          role: profile.role,
          teamId: profile.teamId ?? null,
          teamName: profile.teamName ?? null,
        };
      });
    } catch (error) {
      console.error('Failed to hydrate user profile', error);
      logout();
      throw error;
    }
  }, [logout]);

  const applyAuthSession = useCallback(
    (authPayload: LoginResponse) => {
      authStorage.setTokens(authPayload.accessToken, authPayload.refreshToken);
      setAccessToken(authPayload.accessToken);
      setRefreshToken(authPayload.refreshToken);
      setRefreshTokenExpiresOnUtc(authPayload.refreshTokenExpiresOnUtc);

      const expiresIsoFromClaims = applyClaimsFromToken(authPayload.accessToken);
      const effectiveExpiry = expiresIsoFromClaims ?? authPayload.accessTokenExpiresOnUtc;
      scheduleRefresh(effectiveExpiry);

      setUser((prev) => {
        const base: AuthenticatedUser =
          prev ?? ({
            userId: authPayload.userId,
            email: authPayload.email,
            fullName: authPayload.fullName,
            teamId: authPayload.teamId ?? null,
          } as AuthenticatedUser);

        return {
          ...base,
          userId: authPayload.userId,
          email: authPayload.email,
          fullName: authPayload.fullName,
          role: authPayload.role,
          teamId: authPayload.teamId ?? null,
          accessTokenExpiresOnUtc: authPayload.accessTokenExpiresOnUtc,
          refreshTokenExpiresOnUtc: authPayload.refreshTokenExpiresOnUtc,
        };
      });
    },
    [applyClaimsFromToken, scheduleRefresh],
  );

  const refreshSession = useCallback(async () => {
    if (isRefreshingRef.current) {
      return;
    }

    if (!refreshToken) {
      logout();
      return;
    }

    isRefreshingRef.current = true;
    try {
      const result = await authService.refresh({ refreshToken });
      applyAuthSession(result.data);
      void hydrateProfile();
    } catch (error) {
      console.error('Token refresh failed', error);
      logout();
      throw error;
    } finally {
      isRefreshingRef.current = false;
    }
  }, [applyAuthSession, hydrateProfile, logout, refreshToken]);

  useEffect(() => {
    refreshSessionRef.current = refreshSession;
  }, [refreshSession]);

  const login = useCallback(
    async (credentials: LoginRequest) => {
      setIsAuthenticating(true);
      try {
        const result = await authService.login(credentials);
        applyAuthSession(result.data);
        await hydrateProfile();
        return result.data;
      } finally {
        setIsAuthenticating(false);
      }
    },
    [applyAuthSession, hydrateProfile],
  );

  useEffect(() => {
    let isMounted = true;

    const bootstrap = async () => {
      const storedAccess = authStorage.getAccessToken();
      const storedRefresh = authStorage.getRefreshToken();

      if (!storedAccess || !storedRefresh) {
        if (isMounted) {
          setIsHydrating(false);
        }
        return;
      }

      setAccessToken(storedAccess);
      setRefreshToken(storedRefresh);

      const expiry = applyClaimsFromToken(storedAccess);
      scheduleRefresh(expiry);

      try {
        await hydrateProfile();
      } catch {
        // handled inside hydrateProfile (logout already invoked)
      } finally {
        if (isMounted) {
          setIsHydrating(false);
        }
      }
    };

    bootstrap();

    return () => {
      isMounted = false;
      clearRefreshTimer();
    };
  }, [applyClaimsFromToken, clearRefreshTimer, hydrateProfile, scheduleRefresh]);

  const value = useMemo(
    () => ({
      accessToken,
      refreshToken,
      accessTokenExpiresOnUtc,
      refreshTokenExpiresOnUtc,
      user,
      isAuthenticated: Boolean(accessToken),
      isAuthenticating,
      isHydrating,
      login,
      logout,
      refresh: refreshSession,
    }),
    [
      accessToken,
      refreshToken,
      accessTokenExpiresOnUtc,
      refreshTokenExpiresOnUtc,
      user,
      isAuthenticating,
      isHydrating,
      login,
      logout,
      refreshSession,
    ],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }

  return context;
};
