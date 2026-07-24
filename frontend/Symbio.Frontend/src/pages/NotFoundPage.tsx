import React from 'react';
import { Link } from 'react-router-dom';

export const NotFoundPage: React.FC = () => (
  <main style={{ padding: '2rem', fontFamily: 'Arial, sans-serif', textAlign: 'center' }}>
    <h1>404</h1>
    <p>Page not found. Return to the public landing page.</p>
    <Link to="/" style={{ color: '#0072ce' }}>Go home</Link>
  </main>
);
