import React, { useState, useEffect } from 'react';

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
        <div className="min-h-screen bg-gradient-to-br from-gray-50 to-gray-100 py-8">
            <div className="max-w-6xl mx-auto px-4">
                <h1 className="text-4xl font-bold text-primary text-center mb-8">Admin Dashboard - Product Ratings Analytics</h1>
                
                <div className="space-y-6">
                    {analytics.map(product => (
                        <div key={product.productId} className="bg-white rounded-2xl shadow-lg border border-gray-100 overflow-hidden transition-all duration-300 hover:translate-y-[-3px] hover:shadow-xl">
                            <div className="h-2 bg-gradient-to-r from-accent to-secondary"></div>
                            <div className="p-6">
                                <h3 className="text-2xl font-bold text-gray-800 mb-4">{product.productName}</h3>
                                <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mb-6">
                                    <p className="text-gray-700">
                                        <span className="font-semibold text-primary">Average Rating:</span> {product.averageRating.toFixed(1)}/5
                                    </p>
                                    <p className="text-gray-700">
                                        <span className="font-semibold text-primary">Total Ratings:</span> {product.totalRatings}
                                    </p>
                                </div>
                                
                                <div className="border-t border-gray-200 pt-6">
                                    <h4 className="text-xl font-semibold text-primary mb-4 flex items-center gap-2">
                                        <i className="fas fa-star text-accent"></i>
                                        Recent Ratings
                                    </h4>
                                    <div className="space-y-3">
                                        {product.ratings.map((rating, index) => (
                                            <div key={index} className="bg-gray-50 p-4 rounded-xl border-l-4 border-secondary transition-all duration-300 hover:translate-x-1">
                                                <div className="flex justify-between items-center">
                                                    <span className="text-gray-700 font-medium">
                                                        User {rating.username}: {rating.rating} stars
                                                    </span>
                                                    <small className="text-gray-500 italic">
                                                        on {new Date(rating.date).toLocaleDateString()}
                                                    </small>
                                                </div>
                                            </div>
                                        ))}
                                    </div>
                                </div>
                            </div>
                        </div>
                    ))}
                </div>
            </div>
        </div>
    );
}

export default AdminDashboard;