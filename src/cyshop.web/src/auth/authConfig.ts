export const oidcConfig = {
  authority: 'http://localhost:8080/realms/cyshop',
  client_id: 'basket-api',
  redirect_uri: window.location.origin + '/auth/callback',
  post_logout_redirect_uri: window.location.origin,
  response_type: 'code',
  scope: 'openid profile',
  automaticSilentRenew: true,
};
