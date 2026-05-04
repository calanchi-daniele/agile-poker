import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import './index.css'
import App from './App.tsx'
import { SignalRProvider } from './context/SignalRContext.tsx' // <-- Add this import

createRoot(document.getElementById('root')!).render(
    <StrictMode>
        <SignalRProvider>  {/* <-- Wrap your App with the Provider */}
            <App />
        </SignalRProvider>
    </StrictMode>,
)