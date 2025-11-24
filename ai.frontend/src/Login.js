import React, { useState } from 'react';
import './Login.css';

function Login({ onLogin }) {
    const [username, setUsername] = useState('');
    const [password, setPassword] = useState('');

    const handleLogin = async (e) => {
        e.preventDefault();
        console.log('Attempting login:', { username, password });
        
        try {
            const response = await fetch('http://localhost:7000/api/auth/login', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ username, password })
            });
    
            console.log('Login response status:', response.status);
            
            if (response.ok) {
                const user = await response.json();
                console.log('Login successful:', user);
                localStorage.setItem('user', JSON.stringify(user));
                onLogin(user);
                alert(`Welcome ${user.username}!`);
            } else {
                const error = await response.text();
                console.log('Login failed:', error);
                alert('Login failed! Check console for details.');
            }
        } catch (error) {
            console.error('Login error:', error);
            alert('Login failed! Check console.');
        }
    };

    return (
        <div className="login-container">
            <form onSubmit={handleLogin} className="login-form">
                <h2>Login</h2>
                <input
                    type="text"
                    placeholder="Username"
                    value={username}
                    onChange={(e) => setUsername(e.target.value)}
                    required
                />
                <input
                    type="password"
                    placeholder="Password"
                    value={password}
                    onChange={(e) => setPassword(e.target.value)}
                    required
                />
                <button type="submit">Login</button>
                <div className="demo-accounts">
                    <p><strong>Demo Accounts:</strong></p>
                    <p>Admin: admin / temp123</p>
                    <p>Customer: customer1 / temp123</p>
                </div>
            </form>
        </div>
    );
}

export default Login;