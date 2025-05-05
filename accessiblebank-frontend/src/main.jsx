//React namespace, necessary when working with JSX
import React from 'react'
//API for  rendering React components into the DOM
import ReactDOM from 'react-dom/client'
//main App component which contains the app's routing and page structure
import App from './App.jsx'
//global CSS file to apply base styles across the app
import './index.css'
//component which supplies auth context(JWT token, login, logout) to the app
import { AuthProvider } from './context/AuthContext.jsx'

//Create a React root tied to the HTML element with the id root(in index.html) 
//Calls render to mount the React component tree
ReactDOM.createRoot(document.getElementById('root')).render(
  //Wraps the app in StrictMode, a development tool that highlights potenctial problems and deprecations
  <React.StrictMode>
    {/*Wraps the app in AuthProvider, making auth state and methosds available via context to any component in the tree */}
    <AuthProvider>
      {/*Renders the main App component inside the provider */}
      <App />
    </AuthProvider>
  </React.StrictMode>,
)
