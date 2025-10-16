const ACCESS_TOKEN_KEY = 'ore:access-token';
const REFRESH_TOKEN_KEY = 'ore:refresh-token';

const isBrowser = typeof window !== 'undefined' && typeof window.localStorage !== 'undefined';

const getStorage = () => {
  if (!isBrowser) {
    return undefined;
  }

  return window.localStorage;
};

export const authStorage = {
  getAccessToken(): string | null {
    const storage = getStorage();
    return storage?.getItem(ACCESS_TOKEN_KEY) ?? null;
  },
  getRefreshToken(): string | null {
    const storage = getStorage();
    return storage?.getItem(REFRESH_TOKEN_KEY) ?? null;
  },
  setTokens(accessToken: string, refreshToken: string | null = null) {
    const storage = getStorage();
    if (!storage) {
      return;
    }

    storage.setItem(ACCESS_TOKEN_KEY, accessToken);
    if (refreshToken) {
      storage.setItem(REFRESH_TOKEN_KEY, refreshToken);
    } else {
      storage.removeItem(REFRESH_TOKEN_KEY);
    }
  },
  clear() {
    const storage = getStorage();
    if (!storage) {
      return;
    }

    storage.removeItem(ACCESS_TOKEN_KEY);
    storage.removeItem(REFRESH_TOKEN_KEY);
  },
};
