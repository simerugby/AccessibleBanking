import { useState } from "react";

//Declares and exports the functional component NewTransactionForm
//It accepts 2 props, accounts(array of account objects) and a callback to notify the parent to refresh the transaction list
export default function NewTransactionForm({ accounts, onTransactionCreated }) {
  //State for the "From" account ID as string
  const [from, setFrom] = useState("");
  //State for the "To" account ID
  const [to, setTo] = useState("");
  //State for the transaction amount
  const [amount, setAmount] = useState("");
  //State for the transaction description text
  const [description, setDescription] = useState("");
  //State for the transaction category
  const [category, setCategory] = useState("");

  //Async function to handle form submission
  const handleSubmit = async (e) => {
    //Prevents the default form submission behavior
    e.preventDefault();
    //Retrieves the JWT authentication token from localStorage
    const token = localStorage.getItem("token");

    //Sends a POST request to the /api/transactions endpoint
    const res = await fetch("http://localhost:5129/api/transactions", {
      method: "POST",
      //Set the headers
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${token}`
      },
      //Request body contains
      body: JSON.stringify({
        fromAccountId: parseInt(from), //numeric ID parsed from "From"
        toAccountId: parseInt(to), //numeric ID parsed from "To"
        amount: parseFloat(amount), //floating-point value parsed from "Amount"
        description, //plain text
        category //plain text
      })
    });

    //If response is successful, 1 resets all input state fields to empty strings 2 calls onTransactionCreated to update parent's data
    if (res.ok) {
      setFrom("");
      setTo("");
      setAmount("");
      setDescription("");
      setCategory("");
      onTransactionCreated();
    } else {
      alert("Transaction failed");
    }
  };

  return (
    //Renders a styled <form> element and attaches the handleSubmit function to onSubmit
    <form onSubmit={handleSubmit} className="p-4 border rounded bg-white shadow mb-6">
      <h3 className="text-lg font-bold mb-4">New Transaction</h3>

      <div className="grid grid-cols-2 md:grid-cols-3 gap-4 mb-4">
        {/*Dropdown to select the source account, First option is the placeholder, Maps over accounts to create options showing id and currency  */}
        <select value={from} onChange={(e) => setFrom(e.target.value)} className="border p-2 rounded">
          <option value="">From Account</option>
          {accounts.map((a) => (
            <option key={a.id} value={a.id}>
              {a.id} ({a.currency})
            </option>
          ))}
        </select>

        {/*Dropdown to select the destination account, same as from */}
        <select value={to} onChange={(e) => setTo(e.target.value)} className="border p-2 rounded">
          <option value="">To Account</option>
          {accounts.map((a) => (
            <option key={a.id} value={a.id}>
              {a.id} ({a.currency})
            </option>
          ))}
        </select>

        {/*Number input for the transaction amount, allowing 2 decimal places. Controlled by amount state*/}
        <input
          type="number"
          step="0.01"
          placeholder="Amount"
          value={amount}
          onChange={(e) => setAmount(e.target.value)}
          className="border p-2 rounded"
        />

        {/*Text input for the transaction category */}
        <input
          type="text"
          placeholder="Category"
          value={category}
          onChange={(e) => setCategory(e.target.value)}
          className="border p-2 rounded"
        />

        {/*Text input for an optional description */}
        <input
          type="text"
          placeholder="Description"
          value={description}
          onChange={(e) => setDescription(e.target.value)}
          className="border p-2 rounded"
        />
      </div>

      {/*Submit button */}
      <button
        type="submit"
        className="bg-green-600 text-white px-4 py-2 rounded hover:bg-green-700"
      >
        Send
      </button>
    </form>
  );
}
