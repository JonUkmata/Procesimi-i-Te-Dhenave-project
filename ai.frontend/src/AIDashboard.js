import React, { useState, useEffect } from 'react';
import './AIDashboard.css';

function AIDashboard() {
    const [analysis, setAnalysis] = useState(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        console.log('AIDashboard mounted');
        fetchAIAnalysis();
    }, []);

    const fetchAIAnalysis = async () => {
        try {
            console.log('Fetching AI analysis...');
            const response = await fetch('http://localhost:7000/api/analytics/ai-analysis');
            console.log('Response status:', response.status);
            
            if (!response.ok) throw new Error('HTTP error: ' + response.status);
            
            const data = await response.json();
            console.log('AI analysis received:', data);
            
            setAnalysis(data);
        } catch (error) {
            console.error('Error:', error);
            setAnalysis(null);
        } finally {
            setLoading(false);
        }
    };

    if (loading) {
        return (
            <div className="dashboard-loading">
                <div className="loading-spinner"></div>
                <p>Loading AI Analysis...</p>
            </div>
        );
    }

    if (!analysis) {
        return (
            <div className="dashboard-error">
                <h2>Analysis Unavailable</h2>
                <p>Failed to load product analysis data</p>
                <button onClick={fetchAIAnalysis} className="retry-btn">Retry</button>
            </div>
        );
    }

    const products = analysis.products || analysis.Products || [];
    const insights = analysis.insights || analysis.Insights || {};

    return (
        <div className="ai-dashboard">
            {/* Header */}
            <div className="dashboard-header">
                <h1>AI Product Intelligence Dashboard</h1>
                <p>Comprehensive analysis of customer ratings and product performance</p>
            </div>

            {/* Business Insights */}
            <div className="insights-section">
                <h2>Business Insights</h2>
                
                <div className="insights-grid">
                    <div className="insight-card satisfaction">
                        <h3>Overall Satisfaction</h3>
                        <div className="big-number">
                            {insights.overallCustomerSatisfaction || insights.OverallCustomerSatisfaction || '0'}/5
                        </div>
                        <p>Average Rating</p>
                    </div>
                    
                    <div className="insight-card products">
                        <h3>Products Analyzed</h3>
                        <div className="big-number">
                            {insights.totalProductsAnalyzed || insights.TotalProductsAnalyzed || '0'}
                        </div>
                        <p>Total Products</p>
                    </div>
                    
                    <div className="insight-card critical">
                        <h3>Needs Attention</h3>
                        <div className="big-number">
                            {insights.criticalProducts || insights.CriticalProducts || '0'}
                        </div>
                        <p>Critical Products</p>
                    </div>
                    
                    <div className="insight-card excellent">
                        <h3>Excellent Products</h3>
                        <div className="big-number">
                            {insights.excellentProducts || insights.ExcellentProducts || '0'}
                        </div>
                        <p>Top Performers</p>
                    </div>
                </div>

                {/* Key Insight */}
                <div className="key-insight">
                    <h3>Key Insight</h3>
                    <p>{insights.keyInsight || insights.KeyInsight || 'No insights available'}</p>
                    <div className="recommendation">
                        <strong>Recommendation:</strong> {insights.recommendation || insights.Recommendation || 'Continue monitoring product performance'}
                    </div>
                </div>
            </div>

            {/* Product Analysis */}
            <div className="products-analysis">
                <h2>Product Performance Analysis</h2>
                <p className="products-count">{products.length} products analyzed</p>
                
                <div className="products-grid">
                    {products.length > 0 ? (
                        products.map((product, index) => {
                            const category = product.category || product.Category || '';
                            const getCategoryIcon = () => {
                                if (category.includes('Excellent')) return 'fas fa-star';
                                if (category.includes('Very Good')) return 'fas fa-thumbs-up';
                                if (category.includes('Average')) return 'fas fa-minus-circle';
                                if (category.includes('Needs Improvement')) return 'fas fa-exclamation-triangle';
                                if (category.includes('Critical')) return 'fas fa-exclamation-circle';
                                return 'fas fa-question-circle';
                            };

                            return (
                                <div key={product.productId || product.ProductId || index} className="product-analysis-card">
                                    <div className="product-header">
                                        <h3>{product.productName || product.ProductName}</h3>
                                        <span className={`category-badge ${category.toLowerCase().replace(/[^a-z]/g, '')}`}>
                                            <i className={getCategoryIcon()}></i>
                                            {category}
                                        </span>
                                    </div>

                                    <div className="product-metrics">
                                        <div className="metric">
                                            <span>Average Rating</span>
                                            <strong>{product.averageRating || product.AverageRating}/5</strong>
                                        </div>
                                        <div className="metric">
                                            <span>Total Ratings</span>
                                            <strong>{product.totalRatings || product.TotalRatings}</strong>
                                        </div>
                                        <div className="metric">
                                            <span>Risk Level</span>
                                            <strong>{product.riskLevel || product.RiskLevel || 'Medium'}</strong>
                                        </div>
                                        <div className="metric">
                                            <span>Sentiment</span>
                                            <strong>{product.sentiment || product.Sentiment || 'Neutral'}</strong>
                                        </div>
                                    </div>

                                    <div className="recommendation">
                                        <strong>Action:</strong> {product.recommendation || product.Recommendation}
                                    </div>

                                    {/* Rating Distribution */}
                                    <div className="rating-distribution">
                                        <h4>Rating Distribution</h4>
                                        {[5, 4, 3, 2, 1].map(star => {
                                            const distribution = product.ratingDistribution || product.RatingDistribution || {};
                                            const count = distribution[star] || 0;
                                            const total = product.totalRatings || product.TotalRatings || 1;
                                            const percentage = (count / total) * 100;
                                            
                                            return (
                                                <div key={star} className="distribution-row">
                                                    <span>{star} stars</span>
                                                    <div className="bar-container">
                                                        <div 
                                                            className="bar" 
                                                            style={{ width: `${percentage}%` }}
                                                        ></div>
                                                    </div>
                                                    <span>{count}</span>
                                                </div>
                                            );
                                        })}
                                    </div>
                                </div>
                            );
                        })
                    ) : (
                        <div className="no-products">
                            <h3>No Products Available</h3>
                            <p>No product data found for analysis</p>
                        </div>
                    )}
                </div>
            </div>
        </div>
    );
}

export default AIDashboard;