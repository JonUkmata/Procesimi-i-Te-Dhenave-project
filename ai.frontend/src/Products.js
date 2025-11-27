import React, { useState, useEffect } from 'react';

function Products({ currentUser }) {
    const [products, setProducts] = useState([]);
    const [comments, setComments] = useState({});
    const [newComments, setNewComments] = useState({});
    const [loading, setLoading] = useState({});

    useEffect(() => {
        fetchProducts();
    }, []);

    const fetchProducts = async () => {
        try {
            const response = await fetch('http://localhost:7000/api/products');
            const data = await response.json();
            setProducts(data);
            
            data.forEach(product => {
                fetchComments(product.id);
            });
        } catch (error) {
            console.error('Error fetching products:', error);
        }
    };

    const fetchComments = async (productId) => {
        try {
            const endpoint = currentUser?.role === 'Admin' 
                ? `http://localhost:7000/api/products/comments-with-sentiment/${productId}`
                : `http://localhost:7000/api/products/comments/${productId}`;
                
            const response = await fetch(endpoint);
            const data = await response.json();
            setComments(prev => ({
                ...prev,
                [productId]: data
            }));
        } catch (error) {
            console.error(`Error fetching comments for product ${productId}:`, error);
        }
    };

    const rateProduct = async (productId, rating) => {
        if (!currentUser) {
            alert('Please login first!');
            return;
        }

        try {
            await fetch('http://localhost:7000/api/products/rate', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    productId: productId,
                    userId: currentUser.userId,
                    rating: rating
                })
            });
            alert('Rating saved!');
        } catch (error) {
            console.error('Error rating product:', error);
        }
    };

    const handleCommentChange = (productId, text) => {
        setNewComments(prev => ({
            ...prev,
            [productId]: text
        }));
    };

    const submitComment = async (productId) => {
        if (!currentUser) {
            alert('Please login first!');
            return;
        }

        const commentText = newComments[productId]?.trim();
        if (!commentText) {
            alert('Please enter a comment');
            return;
        }

        setLoading(prev => ({ ...prev, [productId]: true }));

        try {
            const response = await fetch('http://localhost:7000/api/products/comment', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    productId: productId,
                    userId: currentUser.userId,
                    text: commentText
                })
            });

            if (response.ok) {
                alert(`Comment saved!`);
                setNewComments(prev => ({ ...prev, [productId]: '' }));
                fetchComments(productId);
            } else {
                alert('Failed to save comment');
            }
        } catch (error) {
            console.error('Error submitting comment:', error);
            alert('Error submitting comment');
        } finally {
            setLoading(prev => ({ ...prev, [productId]: false }));
        }
    };

    const shouldShowSentiment = currentUser?.role === 'Admin';

    return (
        <div className="min-h-screen bg-gradient-to-br from-gray-50 to-gray-100 py-8">
            <div className="max-w-7xl mx-auto px-4">
                {/* User Info */}
                <div className="bg-gradient-to-r from-primary to-secondary text-white p-6 rounded-2xl shadow-lg mb-8">
                    {currentUser && (
                        <p className="text-xl font-semibold">
                            Welcome, {currentUser.username} ({currentUser.role})
                        </p>
                    )}
                </div>

                <h1 className="text-4xl font-bold text-primary mb-4">Our Products</h1>
                
                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                    {products.map(product => (
                        <div key={product.id} className="bg-white rounded-2xl shadow-lg border border-gray-100 overflow-hidden transition-all duration-300 hover:translate-y-[-5px] hover:shadow-xl">
                            <div className="h-2 bg-gradient-to-r from-accent to-secondary"></div>
                            <div className="p-6">
                                <h3 className="text-xl font-bold text-gray-800 mb-3">{product.name}</h3>
                                <p className="text-gray-600 mb-2">Price: ${product.price}</p>
                                <p className="text-gray-600 mb-4">Stock: {product.stockQuantity}</p>
                                
                                {/* Rating Section */}
                                <div className="border-t border-gray-200 pt-4 mb-4">
                                    <p className="text-primary font-semibold mb-3">Rate this product:</p>
                                    <div className="flex gap-2 flex-wrap">
                                        {[1, 2, 3, 4, 5].map(star => (
                                            <button 
                                                key={star}
                                                className="flex items-center gap-2 bg-white border-2 border-gray-200 px-4 py-2 rounded-lg transition-all duration-300 hover:border-secondary hover:text-secondary font-medium text-gray-600 flex-1 min-w-[60px] justify-center"
                                                onClick={() => rateProduct(product.id, star)}
                                            >
                                                <i className="fas fa-star text-yellow-500"></i>
                                                {star}
                                            </button>
                                        ))}
                                    </div>
                                </div>

                                {/* Comments Section */}
                                <div className="border-t border-gray-200 pt-4">
                                    <h4 className="text-primary font-semibold mb-3 flex items-center gap-2">
                                        <i className="fas fa-comments text-accent"></i>
                                        Customer Comments
                                        {shouldShowSentiment && (
                                            <span className="text-sm text-accent ml-2">(AI Sentiment Analysis)</span>
                                        )}
                                    </h4>
                                    
                                    {/* Comment Form */}
                                    <div className="mb-4">
                                        <textarea
                                            className="w-full p-4 border-2 border-gray-200 rounded-xl resize-y min-h-[100px] focus:border-secondary focus:ring-2 focus:ring-secondary/20 transition-all duration-300"
                                            placeholder="Share your thoughts about this product..."
                                            value={newComments[product.id] || ''}
                                            onChange={(e) => handleCommentChange(product.id, e.target.value)}
                                            rows="3"
                                        />
                                        <button
                                            className="bg-gradient-to-r from-accent to-secondary text-white px-6 py-3 rounded-xl font-semibold mt-3 transition-all duration-300 hover:translate-y-[-2px] hover:shadow-lg disabled:opacity-50 disabled:cursor-not-allowed flex items-center gap-2"
                                            onClick={() => submitComment(product.id)}
                                            disabled={loading[product.id]}
                                        >
                                            <i className="fas fa-paper-plane"></i>
                                            {loading[product.id] ? 'Posting...' : 'Post Comment'}
                                        </button>
                                    </div>

                                    {/* Comments List */}
                                    <div className="max-h-80 overflow-y-auto space-y-3">
                                        {comments[product.id]?.length > 0 ? (
                                            comments[product.id].map(comment => {
                                                const sentiment = comment.sentiment;
                                                const getCommentStyle = () => {
                                                    if (!shouldShowSentiment) return "border-l-4 border-secondary";
                                                    switch (sentiment?.toLowerCase()) {
                                                        case 'positive': return "border-l-4 border-green-500 bg-gradient-to-r from-green-50 to-green-100";
                                                        case 'negative': return "border-l-4 border-red-500 bg-gradient-to-r from-red-50 to-red-100";
                                                        case 'neutral': return "border-l-4 border-blue-500 bg-gradient-to-r from-blue-50 to-blue-100";
                                                        default: return "border-l-4 border-secondary";
                                                    }
                                                };

                                                return (
                                                    <div key={comment.id} className={`p-4 rounded-lg ${getCommentStyle()} transition-all duration-300 hover:translate-x-1`}>
                                                        <div className="flex justify-between items-start mb-2">
                                                            <div className="font-semibold text-primary flex items-center gap-2">
                                                                <i className="fas fa-user text-sm"></i>
                                                                {comment.username}
                                                            </div>
                                                            {shouldShowSentiment && comment.sentiment && (
                                                                <span className={`text-xs text-white px-2 py-1 rounded-full font-semibold uppercase ${
                                                                    sentiment?.toLowerCase() === 'positive' ? 'bg-green-500' :
                                                                    sentiment?.toLowerCase() === 'negative' ? 'bg-red-500' : 'bg-blue-500'
                                                                }`}>
                                                                    {comment.sentiment}
                                                                </span>
                                                            )}
                                                        </div>
                                                        <p className="text-gray-700 mb-2 leading-relaxed">{comment.text}</p>
                                                        <div className="text-sm text-gray-500 text-right">
                                                            {new Date(comment.createdAt).toLocaleDateString()} at{' '}
                                                            {new Date(comment.createdAt).toLocaleTimeString()}
                                                        </div>
                                                    </div>
                                                );
                                            })
                                        ) : (
                                            <div className="text-center py-6 text-gray-500">
                                                <i className="fas fa-comment-slash text-3xl mb-3 text-gray-300"></i>
                                                <p>No comments yet. Be the first to share your thoughts!</p>
                                            </div>
                                        )}
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

export default Products;