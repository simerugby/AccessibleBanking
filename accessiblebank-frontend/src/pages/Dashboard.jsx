//useState for local state management, useEffect for running side effects on component mount or updates
import { useEffect, useState } from "react";
//Imports TransactionTable component to display transactions
import TransactionTable from "../components/TransactionTable";
//"""""" to create new transactions
import NewTransactionForm from "../components/NewTransactionForm";
//"""""" to create new accounts
import NewAccountForm from "../components/NewAccountForm";

//Declares and exports the Dashboard functional component as the default export
export default function Dashboard() {
  //Initializes accounts state as an empty array to hold the user's account data
  //setAccounts will update this array when new data is fetched
  const [accounts, setAccounts] = useState([]);
  //Initializes a numeric state with 0, changing this key triggers re-fetching of transactions in
  //TransactionTable via its reloasKey prop
  const [transactionsReloadKey, setTransactionsReloadKey] = useState(0);

  //Refresh transactions and accounts
  const fetchTransactions = () => {
    //Increments transactionsReloadKey, causing TransactionTable to re-fetch data
    setTransactionsReloadKey(prev => prev + 1);
    //Refresh accounts list
    fetchAccounts();
  };

  //Fetch the user's accounts from the API
  const fetchAccounts = () => {
    //Retrieves the token form localStorage for auth
    const token = localStorage.getItem("token");
  
    //GET request to the /api/accounts endpoint with the Bearer token
    fetch("http://localhost:5129/api/accounts", {
      headers: {
        Authorization: `Bearer ${token}`
      }
    })
      //Parses the JSON response and calls setAccounts to store the data
      .then((res) => res.json())
      .then(setAccounts)
      .catch((err) => console.error("Error fetching accounts:", err));
  };  

  //Call fetchAccounts once on the component's initial mount
  useEffect(() => {
    fetchAccounts();
  }, []);

  //Begins the JSX to render the dashboard UI
  return (
    <div className="p-4">
      <h1 className="text-2xl font-bold mb-4">Dashboard</h1>

      {/*Renders NewAccountForm, 1 refresh accounts after creation, 2 passes the current accounts array for duplicate-checking */}
      <NewAccountForm 
        onAccountCreated={fetchAccounts} 
        existingAccounts={accounts}
      />

      {/*Iterates over accounts to display each account, 1 a.id is the key for each element
      2 shows the currency, balance and account type */}
      {accounts.map((a) => (
        <div key={a.id} className="border p-2 mb-2 rounded bg-white shadow">
          <div>Currency: {a.currency}</div>
          <div>Balance: {a.balance}</div>
          <div>Type: {a.type}</div>
        </div>
      ))}

      {/*Renders NewTransactionForm, 1 passes accounts to show available accounts, 2 refresh
      both transactions and accounts after a new transaction */}
      <NewTransactionForm 
        accounts={accounts} 
        onTransactionCreated={fetchTransactions} 
      />
      
      {/*Renders TransactionTable passing transactionsReloadKey to trigger re-fetch when it changes */}
      <TransactionTable reloadKey={transactionsReloadKey} />
    </div>
  );
}
