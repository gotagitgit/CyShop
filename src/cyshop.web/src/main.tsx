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
  ...oidcConfig,
  userStore: new WebStorageStateStore({ store: window.sessionStorage }),
})

// Connect the UserManager to the API client for token injection
setUserManager(userManager)

function onSigninCallback(): void {
  // Just clean up the query params; the AuthCallbackPage component
  // handles the React Router navigation once auth state is ready
  window.history.replaceState({}, document.title, '/auth/callback')
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
