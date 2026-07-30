import React, { useEffect, useState } from 'react';
import { apiRequest } from '../lib/apiClient';

interface ProjectScope {
  id: string;
  title: string;
  description: string;
  category: string;
  location: string;
  budget: number;
  clientEmail: string;
  postedAt: string;
  milestones: { title: string; description: string }[];
}

export const MarketplacePage: React.FC = () => {
  const [projects, setProjects] = useState<ProjectScope[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const loadProjects = async () => {
    setIsLoading(true);
    setError(null);

    try {
      const data = await apiRequest<ProjectScope[]>('/api/project');
      setProjects(data);
    } catch {
      setError('Could not load public project scopes.');
    }

    setIsLoading(false);
  };

  useEffect(() => {
    void loadProjects();
  }, []);

  return (
    <main className="symbio-page-main">
      <header>
        <h1 className="symbio-page-title">Demand Marketplace</h1>
        <p>Browse published scope-of-work briefs created by regional SMEs and discover matched demand-generating opportunities.</p>
      </header>

      {isLoading && (
        <div style={{ marginTop: '1rem', padding: '1rem 1.1rem', borderRadius: 12, background: '#f3f6fa', color: '#3a4a5c' }}>
          Loading marketplace projects...
        </div>
      )}

      {error && (
        <div style={{ marginTop: '1rem', color: '#a00' }}>
          {error}
          <div style={{ marginTop: '0.75rem' }}>
            <button type="button" onClick={() => void loadProjects()} style={{ padding: '0.55rem 0.85rem', border: '1px solid #ccd5e3', background: '#fff', borderRadius: 8, cursor: 'pointer' }}>
              Retry
            </button>
          </div>
        </div>
      )}

      <div style={{ marginTop: '1.5rem', display: 'grid', gap: '1.25rem' }}>
        {projects.length === 0 && !error && !isLoading ? (
          <div style={{ padding: '1.5rem', background: '#f7f8fb', borderRadius: 16 }}>
            <p>No marketplace projects are available yet. Log in as an SME to post the first scoped offering.</p>
          </div>
        ) : (
          projects.map(project => (
            <article key={project.id} style={{ padding: '1.5rem', background: '#fff', border: '1px solid #e2e6ed', borderRadius: 16 }}>
              <h2 style={{ margin: '0 0 0.75rem' }}>{project.title}</h2>
              <p style={{ margin: '0 0 1rem', color: '#4a4a4a' }}>{project.description}</p>
              <div style={{ display: 'grid', gap: '0.5rem', marginBottom: '1rem' }}>
                <div><strong>Category:</strong> {project.category}</div>
                <div><strong>Location:</strong> {project.location}</div>
                <div><strong>Budget target:</strong> ${project.budget.toFixed(0)}</div>
                <div><strong>Posted by:</strong> {project.clientEmail}</div>
                <div><strong>Published:</strong> {new Date(project.postedAt).toLocaleDateString()}</div>
              </div>
              <div style={{ display: 'grid', gap: '0.75rem' }}>
                <h3 style={{ margin: 0 }}>Milestones</h3>
                <ul style={{ margin: 0, paddingLeft: '1.25rem' }}>
                  {project.milestones.map((milestone, index) => (
                    <li key={index} style={{ marginBottom: '0.5rem' }}>
                      <strong>{milestone.title}:</strong> {milestone.description}
                    </li>
                  ))}
                </ul>
              </div>
            </article>
          ))
        )}
      </div>
    </main>
  );
};
