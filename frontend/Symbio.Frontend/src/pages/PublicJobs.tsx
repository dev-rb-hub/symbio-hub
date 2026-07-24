import React, { useEffect, useState } from 'react';

interface PublicJob {
  id: number;
  title: string;
  description: string;
  clientName: string;
  clientSurname: string;
  budget: string;
  contactEmail: string;
  postedAt: string;
}

export const PublicJobs: React.FC = () => {
  const [jobs, setJobs] = useState<PublicJob[]>([]);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    fetch('http://localhost:5001/api/jobs/public')
      .then(response => {
        if (!response.ok) {
          throw new Error('Unable to load public jobs at this time.');
        }
        return response.json();
      })
      .then(data => setJobs(data))
      .catch(() => setError('Failed to fetch public jobs.'));
  }, []);

  return (
    <main style={{ padding: '2rem', fontFamily: 'Arial, sans-serif', maxWidth: 900, margin: '0 auto' }}>
      <header>
        <h1>Public Jobs Feed</h1>
        <p>Read-only, masked job listings for unauthenticated visitors, with all sensitive budget and contact details hidden.</p>
      </header>

      {error && <div style={{ color: '#a00', marginBottom: '1rem' }}>{error}</div>}

      <div style={{ display: 'grid', gap: '1.25rem' }}>
        {jobs.map(job => (
          <article key={job.id} style={{ border: '1px solid #d6d9dd', padding: '1.5rem', borderRadius: 16, background: '#fff' }}>
            <h2>{job.title}</h2>
            <p>{job.description}</p>
            <dl style={{ display: 'grid', gap: '0.5rem', marginTop: '1rem' }}>
              <div><strong>Client:</strong> {job.clientName} {job.clientSurname}</div>
              <div><strong>Budget:</strong> {job.budget}</div>
              <div><strong>Contact:</strong> {job.contactEmail}</div>
              <div><strong>Published:</strong> {job.postedAt}</div>
            </dl>
          </article>
        ))}
      </div>
    </main>
  );
};
