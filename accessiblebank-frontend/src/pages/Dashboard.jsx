import { useEffect, useState } from "react";
import TransactionTable from "../components/TransactionTable";
import NewTransactionForm from "../components/NewTransactionForm";
import NewAccountForm from "../components/NewAccountForm";



export default function Dashboard() {
  const [accounts, setAccounts] = useState([]);
  const [transactionsReloadKey, setTransactionsReloadKey] = useState(0);

  const fetchTransactions = () => {
    setTransactionsReloadKey(prev => prev + 1);
    fetchAccounts();
  };

  const fetchAccounts = () => {
    const token = localStorage.getItem("token");
  
    fetch("http://localhost:5129/api/accounts", {
      headers: {
        Authorization: `Bearer ${token}`
      }
    })
      .then((res) => res.json())
      .then(setAccounts)
      .catch((err) => console.error("Error fetching accounts:", err));
  };  

  useEffect(() => {
    fetchAccounts();
  }, []);

  return (
    <div className="p-4">
      <h1 className="text-2xl font-bold mb-4">Dashboard</h1>

      <NewAccountForm 
        onAccountCreated={fetchAccounts} 
        existingAccounts={accounts}
      />

      {accounts.map((a) => (
        <div key={a.id} className="border p-2 mb-2 rounded bg-white shadow">
          <div>Account ID: {a.id}</div>
          <div>Currency: {a.currency}</div>
          <div>Balance: {a.balance}</div>
          <div>Type: {a.type}</div>
        </div>
      ))}

      <NewTransactionForm 
        accounts={accounts} 
        onTransactionCreated={fetchTransactions} 
      />
      
      <TransactionTable reloadKey={transactionsReloadKey} />
    </div>
  );
}
