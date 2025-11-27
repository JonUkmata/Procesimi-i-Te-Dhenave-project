import React, { useState, useEffect } from 'react';

function AIDashboard() {
    const [analysis, setAnalysis] = useState(null);
    const [commentAnalysis, setCommentAnalysis] = useState(null);
    const [detailedCommentAnalysis, setDetailedCommentAnalysis] = useState(null);
    const [loading, setLoading] = useState(true);
    const [activeTab, setActiveTab] = useState('overview');
    const [commentLoading, setCommentLoading] = useState(false);

    useEffect(() => {
        console.log('AIDashboard mounted');
        fetchAIAnalysis();
        fetchCommentAnalysis();
        fetchDetailedCommentAnalysis();
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

    const fetchCommentAnalysis = async () => {
        try {
            const response = await fetch('http://localhost:7000/api/analytics/comment-analysis');
            if (response.ok) {
                const data = await response.json();
                setCommentAnalysis(data);
            }
        } catch (error) {
            console.error('Error fetching comment analysis:', error);
        }
    };

    const fetchDetailedCommentAnalysis = async () => {
        setCommentLoading(true);
        try {
            console.log('Fetching detailed comment analysis...');
            const response = await fetch('http://localhost:7000/api/analytics/detailed-comment-analysis');
            console.log('Detailed comment response status:', response.status);
            
            if (!response.ok) throw new Error('HTTP error: ' + response.status);
            
            const data = await response.json();
            console.log('Detailed comment analysis received:', data);
            
            setDetailedCommentAnalysis(data);
        } catch (error) {
            console.error('Error fetching detailed comment analysis:', error);
            setDetailedCommentAnalysis(null);
        } finally {
            setCommentLoading(false);
        }
    };

    const getSentimentClass = (sentiment) => {
        switch (sentiment?.toLowerCase()) {
            case 'very positive': return 'bg-gradient-to-br from-emerald-600 to-emerald-700';
            case 'positive': return 'bg-gradient-to-br from-emerald-500 to-emerald-600';
            case 'neutral': return 'bg-gradient-to-br from-slate-500 to-slate-600';
            case 'negative': return 'bg-gradient-to-br from-amber-500 to-amber-600';
            case 'very negative': return 'bg-gradient-to-br from-rose-600 to-rose-700';
            default: return 'bg-gradient-to-br from-slate-500 to-slate-600';
        }
    };

    const getCommentItemClass = (sentiment) => {
        const baseClasses = "bg-white p-5 rounded-xl shadow-md border-l-4 transition-all duration-300 hover:translate-x-1 hover:shadow-lg";
        switch (sentiment?.toLowerCase()) {
            case 'very positive': return `${baseClasses} border-emerald-600`;
            case 'positive': return `${baseClasses} border-emerald-500`;
            case 'neutral': return `${baseClasses} border-slate-400`;
            case 'negative': return `${baseClasses} border-amber-500`;
            case 'very negative': return `${baseClasses} border-rose-600`;
            default: return `${baseClasses} border-secondary`;
        }
    };

    const getProperty = (obj, propertyName) => {
        if (!obj) return undefined;
        return obj[propertyName] || 
               obj[propertyName.charAt(0).toUpperCase() + propertyName.slice(1)] ||
               obj[propertyName.charAt(0).toLowerCase() + propertyName.slice(1)];
    };

    const renderDetailedCommentAnalysis = () => {
        if (commentLoading) {
            return (
                <div className="flex flex-col items-center justify-center h-64 text-primary">
                    <div className="w-12 h-12 border-3 border-accent border-t-transparent rounded-full animate-spin mb-4"></div>
                    <p>Loading detailed comment analysis...</p>
                </div>
            );
        }
    
        if (!detailedCommentAnalysis) {
            return (
                <div className="text-center py-16 px-8 bg-white rounded-2xl shadow-lg mx-4">
                    <h2 className="text-2xl font-semibold text-primary mb-4">Comment Analysis Unavailable</h2>
                    <p className="text-gray-600 mb-6">Failed to load comment analysis data</p>
                    <button 
                        onClick={fetchDetailedCommentAnalysis}
                        className="bg-gradient-to-r from-accent to-secondary text-white px-6 py-3 rounded-full font-medium transition-all duration-300 hover:translate-y-[-2px] hover:shadow-lg"
                    >
                        Retry
                    </button>
                </div>
            );
        }
    
        // Safely access nested properties with case-insensitive fallbacks
        const overallStats = detailedCommentAnalysis.OverallStats || detailedCommentAnalysis.overallStats || {};
        const topCommenters = detailedCommentAnalysis.TopCommenters || detailedCommentAnalysis.topCommenters || [];
        const recentComments = detailedCommentAnalysis.RecentComments || detailedCommentAnalysis.recentComments || [];
        const productAnalysis = detailedCommentAnalysis.ProductAnalysis || detailedCommentAnalysis.productAnalysis || [];
    
        // Calculate actual values from the stats object - using getProperty for safety
        const totalComments = getProperty(overallStats, 'totalComments') || 0;
        const veryPositiveComments = getProperty(overallStats, 'veryPositiveComments') || 0;
        const positiveComments = getProperty(overallStats, 'positiveComments') || 0;
        const neutralComments = getProperty(overallStats, 'neutralComments') || 0;
        const negativeComments = getProperty(overallStats, 'negativeComments') || 0;
        const veryNegativeComments = getProperty(overallStats, 'veryNegativeComments') || 0;
        const averageSentimentScore = getProperty(overallStats, 'averageSentimentScore') || 0;
    
        return (
            <div className="bg-white rounded-2xl shadow-lg border border-gray-100 overflow-hidden mx-4 mb-6">
                <div className="p-8">
                    <h2 className="text-2xl font-bold text-primary mb-2 flex items-center gap-3">
                        <i className="fas fa-comments text-accent"></i>
                        Detailed Comment Analysis
                    </h2>
                    
                    {/* Overall Stats - Updated for 5 categories */}
                    <div className="grid grid-cols-2 md:grid-cols-6 gap-3 mb-8">
                        {/* Total Comments */}
                        <div className="bg-white p-4 rounded-xl shadow-md border-l-4 border-primary text-center transition-all duration-300 hover:translate-y-[-2px] hover:shadow-lg">
                            <div className="text-2xl font-bold bg-gradient-to-br from-primary to-secondary bg-clip-text text-transparent">
                                {totalComments}
                            </div>
                            <div className="text-xs text-gray-600 font-semibold uppercase tracking-wide mt-1">Total</div>
                        </div>
                        
                        {/* Very Positive */}
                        <div className="bg-gradient-to-br from-emerald-50 to-emerald-100 p-4 rounded-xl shadow-md border-l-4 border-emerald-600 text-center transition-all duration-300 hover:translate-y-[-2px] hover:shadow-lg">
                            <div className="text-2xl font-bold text-emerald-700">{veryPositiveComments}</div>
                            <div className="text-xs text-gray-600 font-semibold uppercase tracking-wide mt-1">Very Positive</div>
                        </div>
                        
                        {/* Positive */}
                        <div className="bg-gradient-to-br from-emerald-50 to-emerald-100 p-4 rounded-xl shadow-md border-l-4 border-emerald-500 text-center transition-all duration-300 hover:translate-y-[-2px] hover:shadow-lg">
                            <div className="text-2xl font-bold text-emerald-600">{positiveComments}</div>
                            <div className="text-xs text-gray-600 font-semibold uppercase tracking-wide mt-1">Positive</div>
                        </div>
                        
                        {/* Neutral */}
                        <div className="bg-gradient-to-br from-slate-50 to-slate-100 p-4 rounded-xl shadow-md border-l-4 border-slate-400 text-center transition-all duration-300 hover:translate-y-[-2px] hover:shadow-lg">
                            <div className="text-2xl font-bold text-slate-600">{neutralComments}</div>
                            <div className="text-xs text-gray-600 font-semibold uppercase tracking-wide mt-1">Neutral</div>
                        </div>
                        
                        {/* Negative */}
                        <div className="bg-gradient-to-br from-amber-50 to-amber-100 p-4 rounded-xl shadow-md border-l-4 border-amber-500 text-center transition-all duration-300 hover:translate-y-[-2px] hover:shadow-lg">
                            <div className="text-2xl font-bold text-amber-600">{negativeComments}</div>
                            <div className="text-xs text-gray-600 font-semibold uppercase tracking-wide mt-1">Negative</div>
                        </div>
                        
                        {/* Very Negative */}
                        <div className="bg-gradient-to-br from-rose-50 to-rose-100 p-4 rounded-xl shadow-md border-l-4 border-rose-600 text-center transition-all duration-300 hover:translate-y-[-2px] hover:shadow-lg">
                            <div className="text-2xl font-bold text-rose-700">{veryNegativeComments}</div>
                            <div className="text-xs text-gray-600 font-semibold uppercase tracking-wide mt-1">Very Negative</div>
                        </div>
                    </div>
    
                    <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 mb-8">
                        {/* Top Commenters */}
                        <div className="bg-gray-50 p-6 rounded-xl border border-gray-200">
                            <h3 className="text-lg font-semibold text-primary mb-4 flex items-center gap-2">
                                <i className="fas fa-users text-accent"></i>
                                Top Commenters
                            </h3>
                            <div className="space-y-3 max-h-80 overflow-y-auto">
                                {topCommenters.length > 0 ? (
                                    topCommenters.map((commenter, index) => {
                                        const commenterTotal = getProperty(commenter, 'totalComments') || 0;
                                        const commenterPositive = getProperty(commenter, 'positiveComments') || 0;
                                        const commenterNegative = getProperty(commenter, 'negativeComments') || 0;
                                        const commenterScore = getProperty(commenter, 'averageSentimentScore') || 0;
                                        const username = getProperty(commenter, 'username') || 'Unknown User';
    
                                        return (
                                            <div key={getProperty(commenter, 'userId') || index} className="bg-white p-4 rounded-lg shadow-sm border-l-4 border-secondary flex justify-between items-center transition-all hover:translate-x-1">
                                                <div>
                                                    <div className="font-semibold text-gray-800">
                                                        #{index + 1} {username}
                                                    </div>
                                                    <div className="text-sm text-gray-600 flex items-center gap-2">
                                                        {commenterTotal} comments •
                                                        <span className="text-emerald-700 flex items-center gap-1">
                                                            <i className="fas fa-thumbs-up text-xs"></i> {commenterPositive}
                                                        </span> •
                                                        <span className="text-rose-700 flex items-center gap-1">
                                                            <i className="fas fa-thumbs-down text-xs"></i> {commenterNegative}
                                                        </span>
                                                    </div>
                                                </div>
                                                <div className="font-bold text-primary">
                                                    {(commenterScore * 100).toFixed(0)}%
                                                </div>
                                            </div>
                                        );
                                    })
                                ) : (
                                    <div className="text-center py-8 text-gray-500">
                                        <i className="fas fa-user-slash text-4xl mb-3 text-gray-300"></i>
                                        <p>No commenters yet</p>
                                    </div>
                                )}
                            </div>
                        </div>
    
                        {/* Recent Comments */}
                        <div className="bg-gray-50 p-6 rounded-xl border border-gray-200">
                            <h3 className="text-lg font-semibold text-primary mb-4 flex items-center gap-2">
                                <i className="fas fa-clock text-accent"></i>
                                Recent Comments
                            </h3>
                            <div className="space-y-4 max-h-80 overflow-y-auto">
                                {recentComments.length > 0 ? (
                                    recentComments.map(comment => {
                                        const commentText = getProperty(comment, 'text') || 'No comment text';
                                        const commentSentiment = getProperty(comment, 'sentiment') || 'Neutral';
                                        const commentScore = getProperty(comment, 'sentimentScore') || 0;
                                        const username = getProperty(comment, 'username') || 'Unknown User';
                                        const productName = getProperty(comment, 'productName') || 'Unknown Product';
                                        const timeAgo = getProperty(comment, 'timeAgo') || 'Recently';
    
                                        return (
                                            <div key={getProperty(comment, 'id')} className={getCommentItemClass(commentSentiment)}>
                                                <div className="flex justify-between items-start mb-3">
                                                    <div>
                                                        <div className="font-semibold text-primary flex items-center gap-2">
                                                            <i className="fas fa-user text-sm"></i>
                                                            {username}
                                                        </div>
                                                        <div className="text-sm text-secondary flex items-center gap-2">
                                                            <i className="fas fa-cube text-xs"></i>
                                                            {productName}
                                                        </div>
                                                    </div>
                                                    <span className={`text-xs text-white px-3 py-1 rounded-full font-semibold uppercase ${getSentimentClass(commentSentiment)}`}>
                                                        {commentSentiment}
                                                    </span>
                                                </div>
                                                <p className="text-gray-700 mb-3 leading-relaxed" title={getProperty(comment, 'fullText') || commentText}>
                                                    {commentText}
                                                </p>
                                                <div className="flex justify-between items-center text-sm text-gray-500">
                                                    <span className="italic">{timeAgo}</span>
                                                    <span className="font-semibold text-primary">
                                                        Score: {(commentScore * 100).toFixed(1)}%
                                                    </span>
                                                </div>
                                            </div>
                                        );
                                    })
                                ) : (
                                    <div className="text-center py-8 text-gray-500">
                                        <i className="fas fa-comment-slash text-4xl mb-3 text-gray-300"></i>
                                        <p>No comments yet</p>
                                    </div>
                                )}
                            </div>
                        </div>
                    </div>
    
                    {/* Product Comment Analysis */}
                    <div className="bg-gray-50 p-6 rounded-xl border border-gray-200">
                        <h3 className="text-lg font-semibold text-primary mb-6 flex items-center gap-2">
                            <i className="fas fa-chart-bar text-accent"></i>
                            Product Comment Analysis
                        </h3>
                        <div className="space-y-4">
                            {productAnalysis.length > 0 ? (
                                productAnalysis.map(product => {
                                    const productTotal = getProperty(product, 'totalComments') || 0;
                                    const productVeryPositive = getProperty(product, 'veryPositiveComments') || 0;
                                    const productPositive = getProperty(product, 'positiveComments') || 0;
                                    const productNeutral = getProperty(product, 'neutralComments') || 0;
                                    const productNegative = getProperty(product, 'negativeComments') || 0;
                                    const productVeryNegative = getProperty(product, 'veryNegativeComments') || 0;
                                    const productName = getProperty(product, 'productName') || 'Unknown Product';
                                    const overallSentiment = getProperty(product, 'overallSentiment') || 'Neutral';

                                    const totalComments = productTotal || 1;
                                    const veryPositivePercent = (productVeryPositive / totalComments) * 100;
                                    const positivePercent = (productPositive / totalComments) * 100;
                                    const neutralPercent = (productNeutral / totalComments) * 100;
                                    const negativePercent = (productNegative / totalComments) * 100;
                                    const veryNegativePercent = (productVeryNegative / totalComments) * 100;

                                    return (
                                        <div key={getProperty(product, 'productId')} className="bg-white p-5 rounded-xl shadow-md border-l-4 border-secondary flex justify-between items-center transition-all hover:translate-x-1">
                                            <div className="flex-1">
                                                <div className="font-semibold text-gray-800 text-lg mb-2">{productName}</div>
                                                <div className="flex gap-3 text-sm text-gray-600 mb-3 flex-wrap">
                                                    <span className="flex items-center gap-1">
                                                        <i className="fas fa-comment text-secondary"></i>
                                                        {productTotal} total
                                                    </span>
                                                    <span className="text-emerald-700 flex items-center gap-1">
                                                        <i className="fas fa-star"></i>
                                                        {productVeryPositive} very positive
                                                    </span>
                                                    <span className="text-emerald-600 flex items-center gap-1">
                                                        <i className="fas fa-thumbs-up"></i>
                                                        {productPositive} positive
                                                    </span>
                                                    <span className="text-slate-600 flex items-center gap-1">
                                                        <i className="fas fa-minus-circle"></i>
                                                        {productNeutral} neutral
                                                    </span>
                                                    <span className="text-amber-600 flex items-center gap-1">
                                                        <i className="fas fa-thumbs-down"></i>
                                                        {productNegative} negative
                                                    </span>
                                                    <span className="text-rose-700 flex items-center gap-1">
                                                        <i className="fas fa-times-circle"></i>
                                                        {productVeryNegative} very negative
                                                    </span>
                                                </div>
                                                {/* Sentiment Distribution Bar */}
                                                <div className="mt-3">
                                                    <div className="flex h-3 bg-gray-200 rounded-full overflow-hidden mb-2">
                                                        <div 
                                                            className="bg-emerald-700 transition-all duration-500"
                                                            style={{ width: `${veryPositivePercent}%` }}
                                                            title={`Very Positive: ${productVeryPositive}`}
                                                        ></div>
                                                        <div 
                                                            className="bg-emerald-500 transition-all duration-500"
                                                            style={{ width: `${positivePercent}%` }}
                                                            title={`Positive: ${productPositive}`}
                                                        ></div>
                                                        <div 
                                                            className="bg-slate-500 transition-all duration-500"
                                                            style={{ width: `${neutralPercent}%` }}
                                                            title={`Neutral: ${productNeutral}`}
                                                        ></div>
                                                        <div 
                                                            className="bg-amber-500 transition-all duration-500"
                                                            style={{ width: `${negativePercent}%` }}
                                                            title={`Negative: ${productNegative}`}
                                                        ></div>
                                                        <div 
                                                            className="bg-rose-700 transition-all duration-500"
                                                            style={{ width: `${veryNegativePercent}%` }}
                                                            title={`Very Negative: ${productVeryNegative}`}
                                                        ></div>
                                                    </div>
                                                    <div className="flex justify-between text-xs text-gray-500 font-medium">
                                                        <span className="text-emerald-700">{veryPositivePercent.toFixed(1)}%</span>
                                                        <span className="text-emerald-600">{positivePercent.toFixed(1)}%</span>
                                                        <span className="text-slate-600">{neutralPercent.toFixed(1)}%</span>
                                                        <span className="text-amber-600">{negativePercent.toFixed(1)}%</span>
                                                        <span className="text-rose-700">{veryNegativePercent.toFixed(1)}%</span>
                                                    </div>
                                                </div>
                                            </div>
                                            <div className={`text-white px-4 py-2 rounded-full text-sm font-semibold uppercase ${getSentimentClass(overallSentiment)}`}>
                                                {overallSentiment}
                                            </div>
                                        </div>
                                    );
                                })
                            ) : (
                                <div className="text-center py-8 text-gray-500">
                                    <i className="fas fa-cubes text-4xl mb-3 text-gray-300"></i>
                                    <p>No product comments yet</p>
                                </div>
                            )}
                        </div>
                    </div>
                </div>
            </div>
        );
    };

    if (loading) {
        return (
            <div className="flex flex-col items-center justify-center min-h-screen text-primary">
                <div className="w-16 h-16 border-4 border-accent border-t-transparent rounded-full animate-spin mb-4"></div>
                <p className="text-xl">Loading AI Analysis...</p>
            </div>
        );
    }

    if (!analysis) {
        return (
            <div className="flex flex-col items-center justify-center min-h-screen text-primary p-8">
                <h2 className="text-3xl font-bold mb-4">Analysis Unavailable</h2>
                <p className="text-gray-600 mb-6 text-center">Failed to load product analysis data</p>
                <button 
                    onClick={fetchAIAnalysis}
                    className="bg-gradient-to-r from-accent to-secondary text-white px-8 py-3 rounded-full font-semibold transition-all duration-300 hover:translate-y-[-2px] hover:shadow-lg"
                >
                    Retry Analysis
                </button>
            </div>
        );
    }

    const products = analysis.products || analysis.Products || [];
    const insights = analysis.insights || analysis.Insights || {};

    return (
        <div className="min-h-screen bg-gradient-to-br from-gray-50 to-gray-100">
            {/* Header */}
            <div className="bg-gradient-to-br from-primary via-secondary to-accent text-white py-16 px-8 text-center relative overflow-hidden">
                <div className="absolute inset-0 bg-black bg-opacity-10"></div>
                <div className="relative z-10">
                    <h1 className="text-4xl md:text-5xl font-light mb-4 tracking-tight">AI Product Intelligence Dashboard</h1>
                    <p className="text-xl opacity-90 max-w-2xl mx-auto">Comprehensive analysis of customer ratings and product performance</p>
                </div>
            </div>

            {/* Tabs */}
            <div className="bg-white shadow-sm border-b">
                <div className="max-w-7xl mx-auto flex">
                    <button 
                        className={`flex items-center gap-3 px-8 py-5 font-medium transition-all duration-300 border-b-2 ${
                            activeTab === 'overview' 
                                ? 'text-primary border-accent bg-gradient-to-r from-accent/5 to-secondary/5' 
                                : 'text-gray-500 border-transparent hover:text-primary'
                        }`}
                        onClick={() => setActiveTab('overview')}
                    >
                        <i className="fas fa-chart-line"></i>
                        Overview
                    </button>
                    <button 
                        className={`flex items-center gap-3 px-8 py-5 font-medium transition-all duration-300 border-b-2 ${
                            activeTab === 'comments' 
                                ? 'text-primary border-accent bg-gradient-to-r from-accent/5 to-secondary/5' 
                                : 'text-gray-500 border-transparent hover:text-primary'
                        }`}
                        onClick={() => setActiveTab('comments')}
                    >
                        <i className="fas fa-comments"></i>
                        Comment Analysis
                    </button>
                </div>
            </div>

            {/* Content */}
            <div className="max-w-7xl mx-auto py-8">
                {activeTab === 'overview' && (
                    <>
                        {/* Business Insights */}
                        <div className="bg-white rounded-2xl shadow-lg border border-gray-100 overflow-hidden mx-4 mb-8">
                            <div className="p-8">
                                <h2 className="text-2xl font-bold text-primary mb-6 flex items-center gap-3">
                                    <div className="w-1 h-8 bg-gradient-to-b from-accent to-secondary rounded"></div>
                                    Business Insights
                                </h2>
                                
                                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
                                    {/* Overall Satisfaction */}
                                    <div className="bg-white p-6 rounded-xl shadow-md border-l-4 border-emerald-500 text-center transition-all duration-300 hover:translate-y-[-5px] hover:shadow-lg">
                                        <h3 className="text-gray-600 text-sm uppercase tracking-wide font-semibold mb-4">Overall Satisfaction</h3>
                                        <div className="text-4xl font-bold bg-gradient-to-br from-primary to-secondary bg-clip-text text-transparent">
                                            {getProperty(insights, 'overallCustomerSatisfaction') || '0'}/5
                                        </div>
                                        <p className="text-gray-600 mt-2">Average Rating</p>
                                    </div>
                                    
                                    {/* Products Analyzed */}
                                    <div className="bg-white p-6 rounded-xl shadow-md border-l-4 border-primary text-center transition-all duration-300 hover:translate-y-[-5px] hover:shadow-lg">
                                        <h3 className="text-gray-600 text-sm uppercase tracking-wide font-semibold mb-4">Products Analyzed</h3>
                                        <div className="text-4xl font-bold bg-gradient-to-br from-primary to-secondary bg-clip-text text-transparent">
                                            {getProperty(insights, 'totalProductsAnalyzed') || '0'}
                                        </div>
                                        <p className="text-gray-600 mt-2">Total Products</p>
                                    </div>
                                    
                                    {/* Needs Attention */}
                                    <div className="bg-gradient-to-br from-rose-50 to-rose-100 p-6 rounded-xl shadow-md border-l-4 border-rose-500 text-center transition-all duration-300 hover:translate-y-[-5px] hover:shadow-lg">
                                        <h3 className="text-gray-600 text-sm uppercase tracking-wide font-semibold mb-4">Needs Attention</h3>
                                        <div className="text-4xl font-bold text-rose-700">
                                            {getProperty(insights, 'criticalProducts') || '0'}
                                        </div>
                                        <p className="text-gray-600 mt-2">Critical Products</p>
                                    </div>
                                    
                                    {/* Excellent Products */}
                                    <div className="bg-gradient-to-br from-emerald-50 to-emerald-100 p-6 rounded-xl shadow-md border-l-4 border-emerald-500 text-center transition-all duration-300 hover:translate-y-[-5px] hover:shadow-lg">
                                        <h3 className="text-gray-600 text-sm uppercase tracking-wide font-semibold mb-4">Excellent Products</h3>
                                        <div className="text-4xl font-bold text-emerald-700">
                                            {getProperty(insights, 'excellentProducts') || '0'}
                                        </div>
                                        <p className="text-gray-600 mt-2">Top Performers</p>
                                    </div>
                                </div>

                                {/* Key Insight */}
                                <div className="bg-gradient-to-r from-accent/5 to-secondary/5 p-6 rounded-xl border-l-4 border-primary">
                                    <h3 className="text-xl font-semibold text-primary mb-4 flex items-center gap-3">
                                        <i className="fas fa-lightbulb text-accent"></i>
                                        Key Insight
                                    </h3>
                                    <p className="text-gray-700 text-lg mb-4 leading-relaxed">
                                        {getProperty(insights, 'keyInsight') || 'No insights available'}
                                    </p>
                                    <div className="bg-accent/10 p-4 rounded-lg border-l-3 border-accent text-accent font-medium">
                                        <strong>Recommendation:</strong> {getProperty(insights, 'recommendation') || 'Continue monitoring product performance'}
                                    </div>
                                </div>
                            </div>
                        </div>

                        {/* Product Analysis */}
                        <div className="bg-white rounded-2xl shadow-lg border border-gray-100 overflow-hidden mx-4">
                            <div className="p-8">
                                <h2 className="text-2xl font-bold text-primary mb-2">Product Performance Analysis</h2>
                                <p className="text-secondary mb-6 text-lg">{products.length} products analyzed</p>
                                
                                <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
                                    {products.length > 0 ? (
                                        products.map((product, index) => {
                                            const category = getProperty(product, 'category') || '';
                                            const getCategoryIcon = () => {
                                                if (category.includes('Excellent')) return 'fas fa-star';
                                                if (category.includes('Very Good')) return 'fas fa-thumbs-up';
                                                if (category.includes('Average')) return 'fas fa-minus-circle';
                                                if (category.includes('Needs Improvement')) return 'fas fa-exclamation-triangle';
                                                if (category.includes('Critical')) return 'fas fa-exclamation-circle';
                                                return 'fas fa-question-circle';
                                            };

                                            const getCategoryColor = () => {
                                                if (category.includes('Excellent')) return 'from-emerald-500 to-emerald-600';
                                                if (category.includes('Very Good')) return 'from-primary to-secondary';
                                                if (category.includes('Average')) return 'from-slate-500 to-slate-600';
                                                if (category.includes('Needs Improvement')) return 'from-amber-500 to-amber-600';
                                                if (category.includes('Critical')) return 'from-rose-500 to-rose-600';
                                                return 'from-primary to-secondary';
                                            };

                                            return (
                                                <div key={getProperty(product, 'productId') || index} className="bg-white p-6 rounded-xl shadow-md border-l-4 border-accent transition-all duration-300 hover:translate-y-[-3px] hover:shadow-lg">
                                                    <div className="flex justify-between items-start mb-6">
                                                        <h3 className="text-xl font-semibold text-gray-800 flex-1 mr-4">
                                                            {getProperty(product, 'productName')}
                                                        </h3>
                                                        <span className={`bg-gradient-to-r ${getCategoryColor()} text-white px-4 py-2 rounded-full text-sm font-semibold uppercase flex items-center gap-2`}>
                                                            <i className={getCategoryIcon()}></i>
                                                            {category}
                                                        </span>
                                                    </div>

                                                    <div className="grid grid-cols-2 gap-4 mb-6">
                                                        <div className="bg-gray-50 p-4 rounded-lg border-l-3 border-secondary flex justify-between">
                                                            <span className="text-gray-600 font-medium">Average Rating</span>
                                                            <strong className="text-primary">{getProperty(product, 'averageRating')}/5</strong>
                                                        </div>
                                                        <div className="bg-gray-50 p-4 rounded-lg border-l-3 border-secondary flex justify-between">
                                                            <span className="text-gray-600 font-medium">Total Ratings</span>
                                                            <strong className="text-primary">{getProperty(product, 'totalRatings')}</strong>
                                                        </div>
                                                        <div className="bg-gray-50 p-4 rounded-lg border-l-3 border-secondary flex justify-between">
                                                            <span className="text-gray-600 font-medium">Risk Level</span>
                                                            <strong className="text-primary">{getProperty(product, 'riskLevel') || 'Medium'}</strong>
                                                        </div>
                                                        <div className="bg-gray-50 p-4 rounded-lg border-l-3 border-secondary flex justify-between">
                                                            <span className="text-gray-600 font-medium">Sentiment</span>
                                                            <strong className="text-primary">{getProperty(product, 'sentiment') || 'Neutral'}</strong>
                                                        </div>
                                                    </div>

                                                    <div className="bg-accent/10 p-4 rounded-lg border-l-3 border-accent text-accent font-medium mb-6">
                                                        <strong>Action:</strong> {getProperty(product, 'recommendation')}
                                                    </div>

                                                    {/* Rating Distribution */}
                                                    <div className="border-t border-gray-200 pt-6">
                                                        <h4 className="text-lg font-semibold text-primary mb-4">Rating Distribution</h4>
                                                        {[5, 4, 3, 2, 1].map(star => {
                                                            const distribution = getProperty(product, 'ratingDistribution') || {};
                                                            const count = distribution[star] || 0;
                                                            const total = getProperty(product, 'totalRatings') || 1;
                                                            const percentage = (count / total) * 100;
                                                            
                                                            return (
                                                                <div key={star} className="flex items-center gap-4 mb-3">
                                                                    <span className="text-gray-600 w-16">{star} stars</span>
                                                                    <div className="flex-1 bg-gray-200 rounded-full h-3 overflow-hidden">
                                                                        <div 
                                                                            className="bg-gradient-to-r from-accent to-secondary h-full rounded-full transition-all duration-500"
                                                                            style={{ width: `${percentage}%` }}
                                                                        ></div>
                                                                    </div>
                                                                    <span className="text-gray-700 font-medium w-12 text-right">{count}</span>
                                                                </div>
                                                            );
                                                        })}
                                                    </div>
                                                </div>
                                            );
                                        })
                                    ) : (
                                        <div className="col-span-2 text-center py-12 text-gray-500">
                                            <i className="fas fa-cubes text-5xl mb-4 text-gray-300"></i>
                                            <h3 className="text-2xl font-semibold text-primary mb-2">No Products Available</h3>
                                            <p>No product data found for analysis</p>
                                        </div>
                                    )}
                                </div>
                            </div>
                        </div>
                    </>
                )}

                {activeTab === 'comments' && renderDetailedCommentAnalysis()}
            </div>
        </div>
    );
}

export default AIDashboard;