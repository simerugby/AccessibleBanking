import { Navigate } from "react-router-dom";

export default function ProtectedRoute({ children }) {
  const token = localStorage.getItem("token");

  //If token is not stored in localStorage returns to main page
  if (!token) {
    return <Navigate to="/" replace />;
  }

  return children;
}
