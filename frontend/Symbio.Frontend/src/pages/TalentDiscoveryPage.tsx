import React, { useEffect, useState } from 'react';
import { useAuth } from '../context/AuthContext';
import { apiRequest } from '../lib/apiClient';

interface TalentProfile {
  id: string;
  name: string;
  companyName: string;
  location: string;
  profileSummary: string;
  skills: string[];
  services: string[];
  hourlyRate: number;
  availability: string;
  isVerified: boolean;
  featuredRank: number;
  lastActiveAt: string;
}

export const TalentDiscoveryPage: React.FC = () => {
  const { session } = useAuth();
  const [query, setQuery] = useState('');
  const [skill, setSkill] = useState('');
  const [location, setLocation] = useState('');
  const [profiles, setProfiles] = useState<TalentProfile[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(false);

  const loadProfiles = async (currentQuery: string, currentSkill: string, currentLocation: string) => {
    if (!session) {
      return;
    }

    setIsLoading(true);
    setError(null);

    const search = new URLSearchParams();
    if (currentQuery.trim()) search.set('query', currentQuery.trim());
    if (currentSkill.trim()) search.set('skill', currentSkill.trim());
    if (currentLocation.trim()) search.set('location', currentLocation.trim());

    try
    {
      const data = await apiRequest<TalentProfile[]>(`/api/talent?${search.toString()}`, {
        token: session.token,
      });

      setProfiles(data);
    }
    catch
    {
      setIsLoading(false);
      setError('Could not load talent discovery results.');
      return;
    }

    setIsLoading(false);
  };

  useEffect(() => {
    void loadProfiles('', '', '');
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [session]);

  const handleSubmit = async (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    await loadProfiles(query, skill, location);
  };

  if (!session) {
    return null;
  }

  return (
    <main style={{ padding: '2rem', fontFamily: 'Arial, sans-serif', maxWidth: 1100, margin: '0 auto' }}>
      <header>
        <p style={{ color: '#0072ce', fontWeight: 700, marginBottom: 0 }}>SME workspace</p>
        <h1 style={{ marginTop: '0.35rem' }}>Talent Discovery</h1>
        <p style={{ maxWidth: 760, lineHeight: 1.7, color: '#444' }}>
          Search verified expert profiles by keyword, skill, or location to shortlist the right delivery partner for regional work.
        </p>
      </header>

      <form onSubmit={handleSubmit} style={{ marginTop: '1.5rem', padding: '1.25rem', background: '#fff', border: '1px solid #e2e6ed', borderRadius: 16, display: 'grid', gap: '1rem' }}>
        <div style={{ display: 'grid', gap: '1rem', gridTemplateColumns: 'repeat(auto-fit, minmax(220px, 1fr))' }}>
          <label>
            Search
            <input value={query} onChange={event => setQuery(event.target.value)} placeholder="React, DevOps, dashboards..." style={{ width: '100%', marginTop: '0.5rem', padding: '0.85rem' }} />
          </label>
          <label>
            Skill
            <input value={skill} onChange={event => setSkill(event.target.value)} placeholder="Azure" style={{ width: '100%', marginTop: '0.5rem', padding: '0.85rem' }} />
          </label>
          <label>
            Location
            <input value={location} onChange={event => setLocation(event.target.value)} placeholder="NSW" style={{ width: '100%', marginTop: '0.5rem', padding: '0.85rem' }} />
          </label>
        </div>
        <div style={{ display: 'flex', gap: '0.75rem', flexWrap: 'wrap' }}>
          <button type="submit" style={{ padding: '0.85rem 1.2rem', background: '#0f9d58', color: '#fff', border: 'none', borderRadius: 10, cursor: 'pointer' }}>
            {isLoading ? 'Searching...' : 'Search talent'}
          </button>
          <button
            type="button"
            onClick={() => {
              setQuery('');
              setSkill('');
              setLocation('');
              void loadProfiles('', '', '');
            }}
            style={{ padding: '0.85rem 1.2rem', background: '#f1f3f6', color: '#111', border: '1px solid #d6d9dd', borderRadius: 10, cursor: 'pointer' }}
          >
            Reset
          </button>
        </div>
      </form>

      {error && <div style={{ marginTop: '1rem', color: '#a00' }}>{error}</div>}

      <section style={{ marginTop: '1.5rem', display: 'grid', gap: '1rem' }}>
        {profiles.length === 0 && !error ? (
          <div style={{ padding: '1.5rem', background: '#f7f8fb', borderRadius: 16 }}>
            <p style={{ margin: 0 }}>No experts matched the current filters.</p>
          </div>
        ) : (
          profiles.map(profile => (
            <article key={profile.id} style={{ padding: '1.5rem', background: '#fff', border: '1px solid #e2e6ed', borderRadius: 16, display: 'grid', gap: '1rem' }}>
              <div style={{ display: 'flex', justifyContent: 'space-between', gap: '1rem', flexWrap: 'wrap' }}>
                <div>
                  <h2 style={{ margin: '0 0 0.35rem' }}>{profile.name}</h2>
                  <p style={{ margin: 0, color: '#555' }}>{profile.companyName}</p>
                </div>
                <div style={{ textAlign: 'right', color: '#555' }}>
                  <div>{profile.location}</div>
                  <div style={{ fontWeight: 700 }}>${profile.hourlyRate.toFixed(0)} / hr</div>
                </div>
              </div>

              <p style={{ margin: 0, lineHeight: 1.7, color: '#333' }}>{profile.profileSummary}</p>

              <div style={{ display: 'flex', gap: '0.5rem', flexWrap: 'wrap' }}>
                {profile.skills.map(skillName => (
                  <span key={skillName} style={{ padding: '0.35rem 0.7rem', background: '#eef6ff', color: '#004b8d', borderRadius: 999, fontSize: '0.9rem' }}>
                    {skillName}
                  </span>
                ))}
              </div>

              <div style={{ display: 'grid', gap: '0.5rem', color: '#444' }}>
                <div><strong>Services:</strong> {profile.services.join(', ')}</div>
                <div><strong>Availability:</strong> {profile.availability}</div>
                <div><strong>Verified:</strong> {profile.isVerified ? 'Yes' : 'No'}</div>
                <div><strong>Last active:</strong> {new Date(profile.lastActiveAt).toLocaleDateString()}</div>
              </div>
            </article>
          ))
        )}
      </section>
    </main>
  );
};