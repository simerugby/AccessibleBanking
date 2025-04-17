import { useState } from "react";

export default function NewTransactionForm({ accounts, onTransactionCreated }) {
  const [from, setFrom] = useState("");
  const [to, setTo] = useState("");
  const [amount, setAmount] = useState("");
  const [description, setDescription] = useState("");
  const [category, setCategory] = useState("");

  const handleSubmit = async (e) => {
    e.preventDefault();
    const token = localStorage.getItem("token");

    const res = await fetch("http://localhost:5129/api/transactions", {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${token}`
      },
      body: JSON.stringify({
        fromAccountId: parseInt(from),
        toAccountId: parseInt(to),
        amount: parseFloat(amount),
        description,
        category
      })
    });

    if (res.ok) {
      setFrom("");
      setTo("");
      setAmount("");
      setDescription("");
      setCategory("");
      onTransactionCreated(); // refresh transaction list
    } else {
      alert("Transaction failed");
    }
  };

  return (
    <form onSubmit={handleSubmit} className="p-4 border rounded bg-white shadow mb-6">
      <h3 className="text-lg font-bold mb-4">New Transaction</h3>

      <div className="grid grid-cols-2 md:grid-cols-3 gap-4 mb-4">
        <select value={from} onChange={(e) => setFrom(e.target.value)} className="border p-2 rounded">
          <option value="">From Account</option>
          {accounts.map((a) => (
            <option key={a.id} value={a.id}>
              {a.id} ({a.currency})
            </option>
          ))}
        </select>

        <select value={to} onChange={(e) => setTo(e.target.value)} className="border p-2 rounded">
          <option value="">To Account</option>
          {accounts.map((a) => (
            <option key={a.id} value={a.id}>
              {a.id} ({a.currency})
            </option>
          ))}
        </select>

        <input
          type="number"
          step="0.01"
          placeholder="Amount"
          value={amount}
          onChange={(e) => setAmount(e.target.value)}
          className="border p-2 rounded"
        />

        <input
          type="text"
          placeholder="Category"
          value={category}
          onChange={(e) => setCategory(e.target.value)}
          className="border p-2 rounded"
        />

        <input
          type="text"
          placeholder="Description"
          value={description}
          onChange={(e) => setDescription(e.target.value)}
          className="border p-2 rounded"
        />
      </div>

      <button
        type="submit"
        className="bg-green-600 text-white px-4 py-2 rounded hover:bg-green-700"
      >
        Send
      </button>
    </form>
  );
}
