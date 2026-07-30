import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { apiRequest } from '../lib/apiClient';
import { PaymentCollectionModal } from '../components/payments/PaymentCollectionModal';
import { PinchRuntimeModePanel, PinchRuntimeModeView } from '../components/payments/PinchRuntimeModePanel';

interface Milestone {
  title: string;
  description: string;
}

interface CreatedProject {
  id: string;
  budget: number;
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
  const [isPreApprovalSubmitting, setIsPreApprovalSubmitting] = useState(false);
  const [showPaymentModal, setShowPaymentModal] = useState(false);
  const [pendingProject, setPendingProject] = useState<CreatedProject | null>(null);
  const [preApprovalSuccess, setPreApprovalSuccess] = useState<{ projectId: string; amount: number; modeLabel: string } | null>(null);
  const [preApprovalError, setPreApprovalError] = useState<string | null>(null);
  const [runtimeMode, setRuntimeMode] = useState<PinchRuntimeModeView | null>(null);
  const [isRuntimeModeLoading, setIsRuntimeModeLoading] = useState(false);
  const [hasRuntimeModeError, setHasRuntimeModeError] = useState(false);

  useEffect(() => {
    if (!session) {
      return;
    }

    setIsRuntimeModeLoading(true);
    setHasRuntimeModeError(false);

    apiRequest<PinchRuntimeModeView>('/api/payments/runtime-mode', {
      token: session.token,
    })
      .then(mode => setRuntimeMode(mode))
      .catch(() => {
        setRuntimeMode(null);
        setHasRuntimeModeError(true);
      })
      .finally(() => {
        setIsRuntimeModeLoading(false);
      });
  }, [session]);

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
      const createdProject = await apiRequest<CreatedProject>('/api/project', {
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

      if (budget >= 5000) {
        setPendingProject(createdProject);
        setPreApprovalError(null);
        setShowPaymentModal(true);
        setIsSubmitting(false);
        return;
      }
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

  const handleCapturePreApproval = async (payload: { accountName: string; bsb: string; accountNumber: string }) => {
    if (!session || !pendingProject) {
      return;
    }

    setIsPreApprovalSubmitting(true);
    setError(null);
    setPreApprovalError(null);

    try
    {
      await apiRequest('/api/payments/pre-approvals', {
        method: 'POST',
        token: session.token,
        body: {
          projectId: pendingProject.id,
          milestoneId: milestones[0]?.title || 'Kickoff',
          accountName: payload.accountName,
          bsb: payload.bsb,
          accountNumber: payload.accountNumber,
          amount: pendingProject.budget,
          currency: 'AUD'
        }
      });
    }
    catch
    {
      setIsPreApprovalSubmitting(false);
      const message = 'Project published, but pre-approval failed. Check details and retry. You can also reopen this project later to complete pre-approval before kickoff.';
      setError(message);
      setPreApprovalError(message);
      return;
    }

    setIsPreApprovalSubmitting(false);
    setShowPaymentModal(false);
    setPreApprovalSuccess({
      projectId: pendingProject.id,
      amount: pendingProject.budget,
      modeLabel: runtimeMode?.modeLabel ?? 'Unknown mode'
    });
    setPendingProject(null);
  };

  return (
    <main className="symbio-page-main">
      <header>
        <h1 className="symbio-page-title">Post a New Project</h1>
        <p>Create a concise scope-of-work brief and publish it to the demand marketplace.</p>
      </header>

      {error && <div style={{ color: '#a00', marginTop: '1rem' }}>{error}</div>}

      {preApprovalSuccess && (
        <section style={{ marginTop: '1rem', padding: '1rem 1.1rem', borderRadius: 12, border: '1px solid #bfe6d1', background: '#eefbf3' }}>
          <h2 style={{ margin: '0 0 0.35rem', fontSize: '1.1rem', color: '#0d6f47' }}>Pre-approval captured</h2>
          <p style={{ margin: 0, color: '#23543f' }}>
            Project {preApprovalSuccess.projectId} is now escrow-ready for
            {' '}
            {preApprovalSuccess.amount.toLocaleString('en-AU', { style: 'currency', currency: 'AUD' })}.
          </p>
          <p style={{ margin: '0.6rem 0 0', color: '#23543f' }}>
            Payment mode used: <strong>{preApprovalSuccess.modeLabel}</strong>
          </p>
          <p style={{ margin: '0.45rem 0 0', color: '#23543f' }}>
            Next step: proceed to marketplace or open your SME dashboard to track state transitions.
          </p>
          <div style={{ marginTop: '0.8rem', display: 'flex', gap: '0.65rem', flexWrap: 'wrap' }}>
            <button
              type="button"
              style={{ padding: '0.7rem 0.95rem', borderRadius: 8, border: 'none', background: '#0f9d58', color: '#fff', cursor: 'pointer' }}
              onClick={() => {
                setPreApprovalSuccess(null);
                navigate('/marketplace');
              }}
            >
              Continue to marketplace
            </button>
            <button
              type="button"
              style={{ padding: '0.7rem 0.95rem', borderRadius: 8, border: '1px solid #9fd5b6', background: '#fff', color: '#155f3e', cursor: 'pointer' }}
              onClick={() => {
                setPreApprovalSuccess(null);
                navigate('/sme/dashboard');
              }}
            >
              Open SME dashboard
            </button>
          </div>
        </section>
      )}

      {budget >= 5000 && (
        <section style={{ marginTop: '1rem', padding: '0.9rem 1rem', borderRadius: 12, border: '1px solid #d8e3f4', background: '#f7fbff', color: '#254564' }}>
          Projects at or above {Number(5000).toLocaleString('en-AU', { style: 'currency', currency: 'AUD' })} require a pre-approval capture step before milestone kickoff.
        </section>
      )}

      <PinchRuntimeModePanel runtimeMode={runtimeMode} isLoading={isRuntimeModeLoading} hasError={hasRuntimeModeError} />

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

      <PaymentCollectionModal
        isOpen={showPaymentModal && pendingProject !== null}
        amount={pendingProject?.budget ?? budget}
        projectTitle={title.trim() || 'Untitled project'}
        milestoneCount={milestones.length}
        milestoneId={milestones[0]?.title || 'Kickoff'}
        runtimeModeLabel={runtimeMode?.modeLabel}
        runtimeModeGuidance={runtimeMode?.guidance}
        usesMockResponses={runtimeMode?.usesMockResponses}
        isSubmitting={isPreApprovalSubmitting}
        submissionError={preApprovalError}
        onCancel={() => {
          setShowPaymentModal(false);
          setPendingProject(null);
          navigate('/marketplace');
        }}
        onSubmit={payload => {
          void handleCapturePreApproval(payload);
        }}
      />
    </main>
  );
};
