export interface JwtPayload {
  exp?: number;
  sub?: string;
  email?: string;
  name?: string;
  [key: string]: unknown;
}

const textDecoder = typeof TextDecoder !== 'undefined' ? new TextDecoder() : null;

const decodeBase64Url = (input: string) => {
  const normalized = input.replace(/-/g, '+').replace(/_/g, '/');
  const pad = normalized.length % 4;
  const padded = pad ? normalized + '='.repeat(4 - pad) : normalized;

  const atobImpl = typeof globalThis !== 'undefined' && typeof globalThis.atob === 'function' ? globalThis.atob : null;
  if (!atobImpl) {
    throw new Error('Base64 decoding is not supported in this environment.');
  }

  const binary = atobImpl(padded);
  if (textDecoder) {
    const bytes = Uint8Array.from(binary, (char) => char.charCodeAt(0));
    return textDecoder.decode(bytes);
  }

  let output = '';
  for (let i = 0; i < binary.length; i += 1) {
    output += String.fromCharCode(binary.charCodeAt(i));
  }
  return output;
};

export const decodeJwt = <T extends JwtPayload = JwtPayload>(token: string): T | null => {
  try {
    const [, payloadSegment] = token.split('.');
    if (!payloadSegment) {
      return null;
    }

    const json = decodeBase64Url(payloadSegment);
    return JSON.parse(json) as T;
  } catch (error) {
    console.warn('Failed to decode JWT payload', error);
    return null;
  }
};
