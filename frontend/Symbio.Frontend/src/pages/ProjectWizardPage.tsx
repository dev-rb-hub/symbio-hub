import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { apiRequest } from '../lib/apiClient';

interface Milestone {
  title: string;
  description: string;
}

export const ProjectWizardPage: React.FC = () => {
  const { session } = useAuth();
  const navigate = useNavigate();
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [category, setCategory] = useState('Automate Manual Admin via AI');
  const [location, setLocation] = useState('Regional NSW');
  const [budget, setBudget] = useState(15000);
  const [milestones, setMilestones] = useState<Milestone[]>([
    { title: 'Define scope', description: '' },
    { title: 'Validate requirements', description: '' },
    { title: 'Deliver proof of concept', description: '' }
  ]);
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const updateMilestone = (index: number, field: 'title' | 'description', value: string) => {
    const next = [...milestones];
    next[index] = { ...next[index], [field]: value };
    setMilestones(next);
  };

  const handleSubmit = async (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setError(null);

    if (!session) {
      setError('You must be logged in as an SME to publish a project.');
      return;
    }

    if (!title.trim() || !description.trim() || budget <= 0) {
      setError('Please complete the project title, description, and budget.');
      return;
    }

    setIsSubmitting(true);

    try
    {
      await apiRequest('/api/project', {
        method: 'POST',
        token: session.token,
        body: {
          title,
          description,
          category,
          location,
          budget,
          clientEmail: session.email,
          milestones,
          isPublished: true
        }
      });
    }
    catch
    {
      setIsSubmitting(false);
      setError('Failed to publish the project. Please try again later.');
      return;
    }

    setIsSubmitting(false);

    navigate('/marketplace');
  };

  return (
    <main style={{ padding: '2rem', fontFamily: 'Arial, sans-serif', maxWidth: 900, margin: '0 auto' }}>
      <header>
        <h1>Post a New Project</h1>
        <p>Create a concise scope-of-work brief and publish it to the demand marketplace.</p>
      </header>

      {error && <div style={{ color: '#a00', marginTop: '1rem' }}>{error}</div>}

      <form onSubmit={handleSubmit} style={{ display: 'grid', gap: '1rem', marginTop: '1.5rem' }}>
        <label>
          Project title
          <input value={title} onChange={e => setTitle(e.target.value)} required style={{ width: '100%', marginTop: '0.5rem', padding: '0.85rem' }} />
        </label>

        <label>
          Project description
          <textarea value={description} onChange={e => setDescription(e.target.value)} rows={5} required style={{ width: '100%', marginTop: '0.5rem', padding: '0.85rem' }} />
        </label>

        <label>
          Category
          <select value={category} onChange={e => setCategory(e.target.value)} style={{ width: '100%', marginTop: '0.5rem', padding: '0.85rem' }}>
            <option value="Automate Manual Admin via AI">Automate Manual Admin via AI</option>
            <option value="Small Business Website Refresh">Small Business Website Refresh</option>
            <option value="Digital Marketing Campaign">Digital Marketing Campaign</option>
            <option value="Data Reporting Dashboard">Data Reporting Dashboard</option>
          </select>
        </label>

        <div style={{ display: 'grid', gap: '1rem', gridTemplateColumns: '1fr 1fr' }}>
          <label>
            Location
            <input value={location} onChange={e => setLocation(e.target.value)} required style={{ width: '100%', marginTop: '0.5rem', padding: '0.85rem' }} />
          </label>

          <label>
            Target budget
            <input type="number" value={budget} onChange={e => setBudget(Number(e.target.value))} min={1000} required style={{ width: '100%', marginTop: '0.5rem', padding: '0.85rem' }} />
          </label>
        </div>

        <fieldset style={{ border: '1px solid #d6d9dd', borderRadius: 12, padding: '1rem' }}>
          <legend style={{ fontWeight: 700 }}>Milestone checklist</legend>
          {milestones.map((milestone, index) => (
            <div key={index} style={{ display: 'grid', gap: '0.75rem', marginBottom: '1rem' }}>
              <label>
                Milestone title
                <input value={milestone.title} onChange={e => updateMilestone(index, 'title', e.target.value)} required style={{ width: '100%', marginTop: '0.5rem', padding: '0.85rem' }} />
              </label>
              <label>
                Milestone details
                <textarea value={milestone.description} onChange={e => updateMilestone(index, 'description', e.target.value)} rows={3} required style={{ width: '100%', marginTop: '0.5rem', padding: '0.85rem' }} />
              </label>
            </div>
          ))}
        </fieldset>

        <button type="submit" disabled={isSubmitting} style={{ padding: '0.85rem 1.25rem', background: '#0f9d58', color: '#fff', border: 'none', borderRadius: 8 }}>
          {isSubmitting ? 'Publishing project...' : 'Publish project'}
        </button>
      </form>
    </main>
  );
};
