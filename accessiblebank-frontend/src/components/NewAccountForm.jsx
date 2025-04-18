import { useState } from "react";

export default function NewAccountForm({ onAccountCreated }) {
  const [currency, setCurrency] = useState("USD");
  const [isLoading, setIsLoading] = useState(false);
  const [accountType, setAccountType] = useState("Regular");

  const handleSubmit = async (e) => {
    e.preventDefault();

    const alreadyExists = existingAccounts?.some(
        (acc) => acc.currency === currency && acc.accountType === accountType
      );
  
      if (alreadyExists) {
        alert(`You already have an account in ${currency}`);
        return;
      }

    setIsLoading(true);

    const token = localStorage.getItem("token");

    try {
      const res = await fetch("http://localhost:5129/api/accounts", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${token}`,
        },
        body: JSON.stringify({ currency, type: accountType}),
      });

      if (!res.ok) throw new Error("Failed to create account");

      setCurrency("USD");
      onAccountCreated?.(); // reload accounts
    } catch (err) {
      console.error("❌ Error creating account:", err);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <form onSubmit={handleSubmit} className="mb-4 p-4 bg-white rounded shadow">
      <h2 className="text-xl font-bold mb-2">Create New Account</h2>
      <select
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
      </select>
      <button
        type="submit"
        className="bg-green-600 text-white px-4 py-2 rounded hover:bg-green-700"
        disabled={isLoading}
      >
        {isLoading ? "Creating..." : "Create Account"}
      </button>
    </form>
  );
}
