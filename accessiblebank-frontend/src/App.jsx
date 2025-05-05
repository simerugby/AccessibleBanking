//Routing components, 1wraps the app and enables HTML5 history API-based routing
import { BrowserRouter, Routes, Route } from "react-router-dom";
//component for the login route
import LoginPage from "./pages/LoginPage";
//component for the protected dashboard route
import Dashboard from "./pages/Dashboard";
//component which guards routes based on auth
import ProtectedRoute from "./components/ProtectedRoute";

//Declares and exports the App functional component as the default app entry point
export default function App() {
  return (
    //Wraps the entire app in BrowserRouter to enable client-side routing
    <BrowserRouter>
      <Routes>
        {/*Defines a route, 1 the root URL, 2 renders the LoginPage component when the URL matches "/" */}
        <Route path="/" element={<LoginPage />} />
        {/*Defines the /dashboard, 1wraps the Dashboard component inside ProtectedRoute
        2ProtectedRoute checks auth and only renders children if the user is logged in */}
        <Route
          path="/dashboard"
          element={
            <ProtectedRoute>
              <Dashboard />
            </ProtectedRoute>
          }
        />
      </Routes>
    </BrowserRouter>
  );
}
