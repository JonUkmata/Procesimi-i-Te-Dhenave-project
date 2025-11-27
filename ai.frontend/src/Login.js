import React, { useState } from 'react';

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
        <div className="min-h-screen bg-gradient-to-br from-primary via-secondary to-accent flex items-center justify-center p-4 relative overflow-hidden">
            {/* Background Pattern - Simplified */}
            <div className="absolute inset-0 bg-black/10"></div>
            
            <form onSubmit={handleLogin} className="bg-white/95 backdrop-blur-sm p-8 rounded-2xl shadow-2xl w-full max-w-md border border-white/20 relative z-10">
                <h2 className="text-3xl font-bold text-primary text-center mb-8">Login</h2>
                
                <div className="space-y-4">
                    <input
                        type="text"
                        placeholder="Username"
                        value={username}
                        onChange={(e) => setUsername(e.target.value)}
                        className="w-full p-4 border-2 border-gray-200 rounded-xl focus:border-secondary focus:ring-2 focus:ring-secondary/20 transition-all duration-300"
                        required
                    />
                    <input
                        type="password"
                        placeholder="Password"
                        value={password}
                        onChange={(e) => setPassword(e.target.value)}
                        className="w-full p-4 border-2 border-gray-200 rounded-xl focus:border-secondary focus:ring-2 focus:ring-secondary/20 transition-all duration-300"
                        required
                    />
                </div>
                
                <button 
                    type="submit"
                    className="w-full bg-gradient-to-r from-accent to-secondary text-white py-4 rounded-xl font-semibold text-lg transition-all duration-300 hover:translate-y-[-2px] hover:shadow-xl mt-6"
                >
                    Login
                </button>
                
                <div className="mt-6 p-4 bg-gradient-to-r from-primary/5 to-secondary/5 rounded-xl border-l-4 border-primary">
                    <p className="text-primary font-semibold mb-2">Demo Accounts:</p>
                    <p className="text-gray-600 text-sm mb-1">Admin: admin / temp123</p>
                    <p className="text-gray-600 text-sm">Customer: customer1 / temp123</p>
                </div>
            </form>
        </div>
    );
}

export default Login;