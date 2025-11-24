import React, { useState, useEffect } from 'react';
import './Products.css';

function Products({ currentUser }) {
    const [products, setProducts] = useState([]);

    useEffect(() => {
        fetchProducts();
    }, []);

    const fetchProducts = async () => {
        try {
            const response = await fetch('http://localhost:7000/api/products');
            const data = await response.json();
            setProducts(data);
        } catch (error) {
            console.error('Error fetching products:', error);
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

    return (
        <div className="products-container">
            <div className="user-info">
                {currentUser && <p>Welcome, {currentUser.username} ({currentUser.role})</p>}
            </div>
            <h1>Our Products</h1>
            <div className="products-grid">
                {products.map(product => (
                    <div key={product.id} className="product-card">
                        <h3>{product.name}</h3>
                        <p>Price: ${product.price}</p>
                        <p>Stock: {product.stockQuantity}</p>
                        
                        <div className="rating-section">
                            <p>Rate this product:</p>
                            <div className="rating-stars">
                                {[1, 2, 3, 4, 5].map(star => (
                                    <button 
                                        key={star}
                                        className="star-btn"
                                        onClick={() => rateProduct(product.id, star)}
                                    >
                                        <i className="fas fa-star"></i>
                                        {star}
                                    </button>
                                ))}
                            </div>
                        </div>
                    </div>
                ))}
            </div>
        </div>
    );
}

export default Products;