import React, { useState, useEffect } from 'react';
import Login from './Login';
import Products from './Products';
import AIDashboard from './AIDashboard';
import './App.css';

function App() {
    const [currentUser, setCurrentUser] = useState(null);

    useEffect(() => {
        const savedUser = localStorage.getItem('user');
        if (savedUser) {
            setCurrentUser(JSON.parse(savedUser));
        }
    }, []);

    const handleLogin = (user) => {
        setCurrentUser(user);
    };

    const handleLogout = () => {
        localStorage.removeItem('user');
        setCurrentUser(null);
    };

    return (
        <div className="App">
            {!currentUser ? (
                <Login onLogin={handleLogin} />
            ) : (
                <div className="min-h-screen bg-gray-50">
                    {/* Navbar */}
                    <nav className="bg-gradient-to-r from-primary to-secondary text-white px-4 py-4 shadow-lg sticky top-0 z-50">
                        <div className="max-w-7xl mx-auto flex justify-between items-center">
                            <div className="flex items-center gap-3">
                                <i className="fas fa-chart-line text-accent text-xl"></i>
                                <span className="font-semibold text-lg">Analytics Dashboard</span>
                            </div>
                            
                            <div className="flex items-center gap-4">
                                <div className="bg-white/15 px-4 py-2 rounded-lg border border-white/10 flex items-center gap-3">
                                    <i className="fas fa-user text-white/90"></i>
                                    <span className="font-medium">{currentUser.username}</span>
                                    <span className="bg-accent/40 text-xs px-2 py-1 rounded-full capitalize">
                                        {currentUser.role}
                                    </span>
                                </div>
                                <button 
                                    className="bg-accent/90 hover:bg-accent w-10 h-10 rounded-lg flex items-center justify-center transition-all duration-300 hover:translate-y-[-1px] border border-white/20"
                                    onClick={handleLogout}
                                    title="Logout"
                                >
                                    <i className="fas fa-sign-out-alt text-white"></i>
                                </button>
                            </div>
                        </div>
                    </nav>
                    
                    {/* Main Content */}
                    {currentUser.role === 'Admin' ? (
                        <AIDashboard />
                    ) : (
                        <Products currentUser={currentUser} />
                    )}
                </div>
            )}
        </div>
    );
}

export default App;