import React, { useState, useEffect } from 'react';
import Login from './Login';
import Products from './Products';
import AIDashboard from './AIDashboard';
import './App.css';

function App() {
    const [currentUser, setCurrentUser] = useState(null);

    useEffect(() => {
        // Check if user is already logged in
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
                <div>
                    <nav className="navbar">
                        <div className="nav-left">
                            <i className="fas fa-chart-line"></i>
                            <span className="nav-title">Analytics Dashboard</span>
                        </div>
                        
                        <div className="nav-right">
                            <div className="user-badge">
                                <i className="fas fa-user"></i>
                                <span className="user-name">{currentUser.username}</span>
                                <span className="user-role">{currentUser.role}</span>
                            </div>
                            <button className="logout-btn" onClick={handleLogout} title="Logout">
                                <i className="fas fa-sign-out-alt"></i>
                            </button>
                        </div>
                    </nav>
                    
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