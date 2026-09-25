// main.ts – Vue application bootstrap.
// Related: src/router.ts (router with guards)
//          src/stores/auth.ts (initialize session on app start)
//          src/App.vue (root component)
//          src/styles/main.css (global CSS variables and base styles)

import { createApp } from 'vue'
import { createPinia } from 'pinia'
import { router } from './router'
import App from './App.vue'
import './styles/main.css'

const app = createApp(App)

// Register Pinia for state management (auth store, ATS store).
app.use(createPinia())

// Register Vue Router with all route definitions and guards.
app.use(router)

app.mount('#app')
