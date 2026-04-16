import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { Provider } from 'react-redux'
import { AuthProvider } from 'react-oidc-context'
import { UserManager, WebStorageStateStore } from 'oidc-client-ts'
import './index.css'
import App from './App.tsx'
import { store } from './store/store'
import { oidcConfig } from './auth/authConfig'
import { setUserManager } from './api/apiClient'

const userManager = new UserManager({
  authority: oidcConfig.authority as string,
  client_id: oidcConfig.client_id as string,
  redirect_uri: oidcConfig.redirect_uri as string,
  post_logout_redirect_uri: oidcConfig.post_logout_redirect_uri as string,
  response_type: oidcConfig.response_type as string,
  scope: oidcConfig.scope as string,
  automaticSilentRenew: oidcConfig.automaticSilentRenew as boolean,
  userStore: new WebStorageStateStore({ store: window.sessionStorage }),
})

// Connect the UserManager to the API client for token injection
setUserManager(userManager)

function onSigninCallback(): void {
  // Remove OIDC code/state params from the URL after login callback
  window.history.replaceState({}, document.title, window.location.pathname)
}

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <AuthProvider userManager={userManager} onSigninCallback={onSigninCallback}>
      <Provider store={store}>
        <App />
      </Provider>
    </AuthProvider>
  </StrictMode>,
)
