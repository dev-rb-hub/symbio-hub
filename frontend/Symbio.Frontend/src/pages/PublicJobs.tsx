import React, { useEffect, useState } from 'react';
import { apiRequest } from '../lib/apiClient';

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
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const loadJobs = async () => {
    setIsLoading(true);
    setError(null);

    try {
      const data = await apiRequest<PublicJob[]>('/api/jobs/public');
      setJobs(data);
    } catch {
      setError('Failed to fetch public jobs.');
    }

    setIsLoading(false);
  };

  useEffect(() => {
    void loadJobs();
  }, []);

  return (
    <main style={{ padding: '2rem', fontFamily: 'Arial, sans-serif', maxWidth: 900, margin: '0 auto' }}>
      <header>
        <h1>Public Jobs Feed</h1>
        <p>Read-only, masked job listings for unauthenticated visitors, with all sensitive budget and contact details hidden.</p>
      </header>

      {isLoading && (
        <div style={{ marginBottom: '1rem', padding: '1rem 1.1rem', borderRadius: 12, background: '#f3f6fa', color: '#3a4a5c' }}>
          Loading public jobs feed...
        </div>
      )}

      {error && (
        <div style={{ color: '#a00', marginBottom: '1rem' }}>
          {error}
          <div style={{ marginTop: '0.75rem' }}>
            <button type="button" onClick={() => void loadJobs()} style={{ padding: '0.55rem 0.85rem', border: '1px solid #ccd5e3', background: '#fff', borderRadius: 8, cursor: 'pointer' }}>
              Retry
            </button>
          </div>
        </div>
      )}

      <div style={{ display: 'grid', gap: '1.25rem' }}>
        {!isLoading && !error && jobs.length === 0 && (
          <article style={{ border: '1px solid #d6d9dd', padding: '1.5rem', borderRadius: 16, background: '#fff' }}>
            <p style={{ margin: 0 }}>No public jobs are available yet. Check back shortly for new openings.</p>
          </article>
        )}

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
