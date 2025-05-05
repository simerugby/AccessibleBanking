//useState allows the component to manage and update its own state
import { useState } from "react";

//Declares a functional React component, with 2 props 1st callback to notify the parent when account is created 2nd array with user's accounts
export default function NewAccountForm({ onAccountCreated, existingAccounts}) {
  //State variables with their default value
  const [currency, setCurrency] = useState("USD");
  //Boolean state isLoading to track if the form is in a loading/submitting state
  const [isLoading, setIsLoading] = useState(false);
  //lets the user choose between account types
  const [accountType, setAccountType] = useState("Regular");

  //asynchronous function that will handle form submission
  const handleSubmit = async (e) => {
    //Prevents the brownser0s default form submission, allowing custom handling in JavaScript
    e.preventDefault();

    //Checks if exists an account eith the selected currency and type in user array of accounts
    const alreadyExists = existingAccounts.some(
        (acc) => acc.currency === currency && acc.accountType === accountType
    );
  
    //If a duplicate account is detected, displays and aborts submission
    if (alreadyExists) {
      alert(`You already have a ${accountType} account in ${currency}`);
      return;
    }

    //Loading state = true to disable the form and show feedback
    setIsLoading(true);
    //Retrieves the user's JWT token from localStorage for authentication
    const token = localStorage.getItem("token");

    if (!token) {
      alert("You must be logged in to create an account.");
      return;
    }
    

    try {

      //Sends a POST request to the API endpoint to create a new account, it includes JSON headers and the Bearer token
      const res = await fetch("http://localhost:5129/api/accounts", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${token}`,
        },
        body: JSON.stringify({ 
          currency, 
          type: accountType === "Savings"
            ? 1 //AccountType.Savings
            : 0 //AccountType.Regular
        }),
      });

      if (!res.ok) throw new Error("Failed to create account");

      //Calls the parent-provider callback to reload or refresh the account list
      onAccountCreated();
      //Resets the currency dropdown back to its default value
      setCurrency("USD");
      //Resets the account type dropdown back to its default value
      setAccountType("Regular");
      //Optionally calls the callback again using the safe navigation operator
      onAccountCreated?.();
    } catch (err) {
      console.error("Error creating account:", err);
      alert("Unable to create account.");
    } finally {
      setIsLoading(false); //reset isLoading to false, for re-enabling the form
    }
  };

  //Defining the UI components
  return (
    //Rendering a form element with Tailwind CSS classes, attach handleSubmit to the form's onSubmit event
    <form onSubmit={handleSubmit} className="mb-4 p-4 bg-white rounded shadow">
      <h2 className="text-xl font-bold mb-2">Create New Account</h2>
      <select
        //value ties dropdown to the accountType state, and onChange updates that state
        value={accountType}
        onChange={(e) => setAccountType(e.target.value)}
        className="border p-2 rounded"
      >
        <option value="Regular">Regular</option>
        <option value="Savings">Savings</option>
      </select>
    
      <select
        value={currency}
        onChange={(e) => setCurrency(e.target.value)}
        className="border p-2 rounded mr-2"
      >
        <option value="USD">USD</option>
        <option value="EUR">EUR</option>
        <option value="AED">AED</option>
        <option value="CNY">CNY</option>
        <option value="JPY">JPY</option>
        <option value="RUB">RUB</option>
        <option value="BTC">BTC</option>
      </select>

      <button
        //submit botton, when isLoading is true the button is disabled and its label changes to creating...
        type="submit"
        className="bg-green-600 text-white px-4 py-2 rounded hover:bg-green-700"
        disabled={isLoading}
      >
        {isLoading ? "Creating..." : "Create Account"}
      </button>
    </form>
  );
}
