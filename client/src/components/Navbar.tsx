import React from 'react';
import { Link, useLocation } from 'react-router-dom';
import './Navbar.css';

const Navbar: React.FC = () => {
  const location = useLocation();

  return (
    <nav className="navbar">
      <div className="navbar-container">
        <Link to="/" className="navbar-brand">
          Clothing E-commerce
        </Link>
        
        <ul className="navbar-menu">
          <li className="navbar-item">
            <Link 
              to="/" 
              className={`navbar-link ${location.pathname === '/' ? 'active' : ''}`}
            >
              Home
            </Link>
          </li>
          <li className="navbar-item">
            <Link 
              to="/add-product" 
              className={`navbar-link ${location.pathname === '/add-product' ? 'active' : ''}`}
            >
              Add Product
            </Link>
          </li>
        </ul>
      </div>
    </nav>
  );
};

export default Navbar;
