//React functions, 1 Create a context object for passing data through the component tree 2 Hook for local state in the provider
import { createContext, useState} from "react";

//Creates and expots AuthContext, which hold authentication-related data and methods
export const AuthContext = createContext();

//Defines and exports the component that uses AuthContent.Provider to wrap its children
export const AuthProvider = ({ children }) => {
  //Initializes token state with the JWT, or null if absent
  const [token, setToken] = useState(localStorage.getItem("token") || null);

  //Defines login function, 1Stores the provided token in localStorage 2 Updates the token state
  const login = (newToken) => {
    localStorage.setItem("token", newToken);
    setToken(newToken);
  };

  //Defines logout function, 1Removes the token from localStorage 2 Clears the token state
  const logout = () => {
    localStorage.removeItem("token");
    setToken(null);
  };

  //Renders the AuthContext.Provider component, 
  //Passes an object as context value, making the current token and auth methods available to consumers
  //Renders nested children inside the provider
  return (
    <AuthContext.Provider value={{ token, login, logout }}>
      {children}
    </AuthContext.Provider>
  );
};
