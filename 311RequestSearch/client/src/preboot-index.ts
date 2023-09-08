export async function prebootTylerWebApp(configPromise: Promise<any>) {
  const { isAppAvailable, redirectToSignIn } = await fetch('api/AppAvailability')
    .then(r => r.json())
    .catch(() => {
      console.warn('Unable to check app availability');
      return { isAppAvailable: false };
    });

  if (!isAppAvailable) {
    console.warn('App is not available');
    const config = await configPromise;
    window.location.assign(config.notFoundPageUri);
  } else if (redirectToSignIn) {
    window.location.replace(`signin?redirectUrl=${encodeURIComponent(window.location.href)}`);
  }

  return { isAppAvailable, redirectToSignIn };
}
