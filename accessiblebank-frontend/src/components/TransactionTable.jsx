import { useEffect, useState } from "react";

export default function TransactionTable({ reloadKey }) {
  const [transactions, setTransactions] = useState([]);
  const [minAmount, setMinAmount] = useState("");
  const [maxAmount, setMaxAmount] = useState("");
  const [category, setCategory] = useState("");
  const [dateFrom, setDateFrom] = useState("");
  const [dateTo, setDateTo] = useState("");

  const token = localStorage.getItem("token");
  console.log("🔑 Token:", token);

  const fetchTransactions = () => {
    const query = new URLSearchParams();
    if (minAmount) query.append("minAmount", minAmount);
    if (maxAmount) query.append("maxAmount", maxAmount);
    if (category) query.append("category", category);
    if (dateFrom) query.append("dateFrom", dateFrom);
    if (dateTo) query.append("dateTo", dateTo);

    fetch(`http://localhost:5129/api/transactions/my?${query.toString()}`, {
      headers: { Authorization: `Bearer ${token}` }
    })
      .then((res) => res.json())
      .then(setTransactions)
      .catch((err) => console.error("❌ Error loading transactions", err));
  };

  useEffect(() => {
    fetchTransactions();
  }, [reloadKey]);

const handleExport = async (format) => {
    try {
        const token = localStorage.getItem("token");

        // Build the query string based on your current filters
        const params = new URLSearchParams();
        params.append("format", format);
        if (category) params.append("category", category);
        if (dateFrom) params.append("dateFrom", dateFrom);
        if (dateTo) params.append("dateTo", dateTo);
        if (minAmount) params.append("minAmount", minAmount);
        if (maxAmount) params.append("maxAmount", maxAmount);

        const response = await fetch(`http://localhost:5129/api/transactions/export?${params.toString()}`, {
            headers: { Authorization: `Bearer ${token}` }
        });
    
        if (!response.ok) {
            throw new Error("Export failed");
        }
    
        const blob = await response.blob();
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement("a");
        a.href = url;
        a.download = `transactions.${format}`;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
    } catch (error) {
        console.error("❌ Export failed:", error);
    }
};
  

  return (
    <div className="mt-4">
      <h2 className="text-xl font-bold mb-2">Recent Transactions</h2>

      {/* Filters */}
      <div className="grid grid-cols-2 md:grid-cols-3 gap-4 mb-4">
        <input
          type="number"
          placeholder="Min amount"
          value={minAmount}
          onChange={(e) => setMinAmount(e.target.value)}
          className="border p-2 rounded"
        />
        <input
          type="number"
          placeholder="Max amount"
          value={maxAmount}
          onChange={(e) => setMaxAmount(e.target.value)}
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
          type="date"
          placeholder="From"
          value={dateFrom}
          onChange={(e) => setDateFrom(e.target.value)}
          className="border p-2 rounded"
        />
        <input
          type="date"
          placeholder="To"
          value={dateTo}
          onChange={(e) => setDateTo(e.target.value)}
          className="border p-2 rounded"
        />
        <button onClick={fetchTransactions} className="bg-blue-600 text-white px-4 py-2 rounded hover:bg-blue-700">
          Apply Filters
        </button>
      </div>

      {/* Export Buttons */}
      <div className="mb-4 flex gap-4">
        <button
            onClick={() => handleExport("csv")}
            className="bg-blue-500 text-white px-4 py-2 rounded hover:bg-blue-600"
        >
            Export to CSV
        </button>
        <button
            onClick={() => handleExport("pdf")}
            className="bg-red-500 text-white px-4 py-2 rounded hover:bg-red-600"
        >
            Export to PDF
        </button>
      </div>


      {/* Table */}
      <table className="w-full border text-sm text-left">
        <thead>
          <tr className="bg-gray-200">
            <th className="p-2 border">From → To</th>
            <th className="p-2 border">Amount</th>
            <th className="p-2 border">Date</th>
            <th className="p-2 border">Description</th>
            <th className="p-2 border">Category</th>
          </tr>
        </thead>
        <tbody>
          {transactions.length > 0 ? (
            transactions.map((t) => (
              <tr key={t.id} className="border-b">
                <td className="p-2 border">{t.fromAccountId} → {t.toAccountId}</td>
                <td className="p-2 border">{t.amount.toFixed(2)}</td>
                <td className="p-2 border">{new Date(t.date).toLocaleDateString()}</td>
                <td className="p-2 border">{t.description}</td>
                <td className="p-2 border">{t.category}</td>
              </tr>
            ))
          ) : (
            <tr>
              <td colSpan="5" className="p-2 text-center">No transactions found.</td>
            </tr>
          )}
        </tbody>
      </table>
    </div>
  );
}