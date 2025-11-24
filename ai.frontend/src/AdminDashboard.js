import React, { useState, useEffect } from 'react';
import './AdminDashboard.css';

function AdminDashboard() {
    const [analytics, setAnalytics] = useState([]);

    useEffect(() => {
        fetchAnalytics();
    }, []);

    const fetchAnalytics = async () => {
        try {
            const response = await fetch('http://localhost:7000/api/analytics/product-ratings');
            const data = await response.json();
            setAnalytics(data);
        } catch (error) {
            console.error('Error fetching analytics:', error);
        }
    };

    return (
        <div className="admin-dashboard">
            <h1>Admin Dashboard - Product Ratings Analytics</h1>
            
            {analytics.map(product => (
                <div key={product.productId} className="product-analytics">
                    <h3>{product.productName}</h3>
                    <p>Average Rating: {product.averageRating.toFixed(1)}/5</p>
                    <p>Total Ratings: {product.totalRatings}</p>
                    
                    <div className="rating-details">
                        <h4>Recent Ratings:</h4>
                        {product.ratings.map((rating, index) => (
                            <div key={index} className="rating-item">
                                <span>User {rating.username}: {rating.rating} stars</span>
                                <small> on {new Date(rating.date).toLocaleDateString()}</small>
                            </div>
                        ))}
                    </div>
                </div>
            ))}
        </div>
    );
}

export default AdminDashboard;