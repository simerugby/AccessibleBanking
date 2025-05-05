//1 to manage local state for form fields 2 to access values from ReactContext
import { useState, useContext } from "react";
//Imports the AuthContext object, which provides auth methods and state
import { AuthContext } from "../context/AuthContext";

//Declare and export LoginPage functional component
export default function LoginPage() {
  //useContext to extract the login function
  const { login } = useContext(AuthContext);
  //Initializes email state to an empty string for the email input, setEmail updates this state when
  //the user types
  const [email, setEmail] = useState("");
  //Initializes password state for the password input, setPassword updates this state
  const [password, setPassword] = useState("");

  //Asynchonous function to handle form submission
  const handleSubmit = async (e) => {
    //Prevents the default browser form submission, allowing custom logic
    e.preventDefault();
    console.log("📤 Sending login request...");
  
    //Sends a POST request to the backend /login endpoint
    //Sets the Content-Type header to JSON
    //The request body contains the credentials entered by the user
    try {
      const res = await fetch("http://localhost:5129/api/users/login", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ email, password })
      });
  
      //Reads the raw response bofy as text, since the API may return either JSON or plain text errors
      const text = await res.text();
      //Variable which holds parsed response data
      let data;
      //Attempts to parse the response text as JSON
      try {
        data = JSON.parse(text);
      } catch {
        data = { error: text };
      }
  
      console.log("🔁 Response:", res.status, data);
  
      //If HTTP status is successful and a token field is present in the response
      if (res.ok && data.token) {
        //Stores the received JWT token in localStorage
        localStorage.setItem("token", data.token); 
        alert("Login successful!");
        //Navigates the browser to the dashboard route after successful login
        window.location.href = "/dashboard"; 
      } else {
        alert("❌ Login failed: " + (data?.error || "No token received"));
      }
    } catch (err) {
      console.error("❌ Network error:", err);
      alert("Network error during login.");
    }
  };
  
  //Begins the JSX to render the login form
  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-100">
      {/*Renders a <form> element, onSubmit handler is attached to handleSubmit */}
      <form onSubmit={handleSubmit} className="bg-white p-6 rounded shadow w-80 space-y-4">
        <h2 className="text-xl font-bold text-center">Login</h2>
        {/*Email input field, 1 controlled by email state 2onChange updates state when the user types */}
        <input
          type="email"
          className="w-full border p-2 rounded"
          placeholder="Email"
          value={email}
          onChange={(e) => setEmail(e.target.value)}
        />
        {/*Password input field, like email */}
        <input
          type="password"
          className="w-full border p-2 rounded"
          placeholder="Password"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
        />
        <button className="w-full bg-blue-500 text-white py-2 rounded hover:bg-blue-600">
          Login
        </button>
      </form>
    </div>
  );
}
