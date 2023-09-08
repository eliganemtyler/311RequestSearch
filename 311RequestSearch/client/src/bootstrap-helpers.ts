export function setCookie(
  name: string,
  value: string,
  expireDelay: number,
  options?: { domain: string; secure: string; crossSite: string }) {
  const date = new Date();
  date.setTime(date.getTime() + expireDelay);
  const expires = `expires=${date.toUTCString()}`;
  const sameSite = options && options.crossSite ? 'none' : 'strict';
  const domain = options && options.domain ? `;domain=${options.domain}` : '';
  const secure = options && options.secure ? ';secure' : '';
  document.cookie = `${name}=${value};${expires};path=/;samesite=${sameSite}${domain}${secure}`;
}

export function deleteCookie(name: string, options?: { domain: string; secure: string; crossSite: string }) {
  setCookie(name, '', 0, options);
}
