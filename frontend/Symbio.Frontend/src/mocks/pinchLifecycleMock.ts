export type LifecycleEventType = 'Payment' | 'Attempt' | 'Transfer';

export type LifecycleEventStatus = 'Queued' | 'Processing' | 'Succeeded' | 'Failed';

export type LifecycleEvent = {
  id: string;
  type: LifecycleEventType;
  status: LifecycleEventStatus;
  atUtc: string;
  amount: number;
  currency: string;
  reference: string;
  notes: string;
};

export type CloseoutEvidence = {
  totalRequired: number;
  uploaded: number;
  verified: number;
};

export type CloseoutReport = {
  title: string;
  value: string;
  detail: string;
};

export type PinchDemoFlow = {
  projectId: string;
  projectTitle: string;
  milestoneId: string;
  payerEmail: string;
  expertEmail: string;
  agreementStatus: 'PendingApproval' | 'Active' | 'Closed';
  settlementState: 'AwaitingEvidence' | 'ReadyToSettle' | 'Settled';
  canSettle: boolean;
  canSettleReason: string;
  escrowVerified: boolean;
  evidence: CloseoutEvidence;
  reports: CloseoutReport[];
  timeline: LifecycleEvent[];
};

export const pinchDemoFlow: PinchDemoFlow = {
  projectId: 'demo-project-epic7-1',
  projectTitle: 'Regional Retail Website Refresh',
  milestoneId: 'Kickoff',
  payerEmail: 'sme@example.com',
  expertEmail: 'expert@example.com',
  agreementStatus: 'Active',
  settlementState: 'ReadyToSettle',
  canSettle: true,
  canSettleReason: 'Required delivery evidence is verified and escrow onboarding is complete.',
  escrowVerified: true,
  evidence: {
    totalRequired: 5,
    uploaded: 5,
    verified: 4,
  },
  reports: [
    {
      title: 'Milestone velocity',
      value: '92%',
      detail: '4 of 5 acceptance checks verified in the current billing window.',
    },
    {
      title: 'Payment confidence',
      value: 'High',
      detail: 'No failed debit attempts in last 14 days for this project.',
    },
    {
      title: 'Settlement risk',
      value: 'Low',
      detail: 'All hard blockers cleared; one advisory note remains for finance review.',
    },
  ],
  timeline: [
    {
      id: 'pay-1',
      type: 'Payment',
      status: 'Queued',
      atUtc: '2026-07-28T01:05:00Z',
      amount: 9500,
      currency: 'AUD',
      reference: 'pay_demo_001',
      notes: 'Milestone settlement request created from agreement workflow.',
    },
    {
      id: 'attempt-1',
      type: 'Attempt',
      status: 'Succeeded',
      atUtc: '2026-07-28T01:06:00Z',
      amount: 9500,
      currency: 'AUD',
      reference: 'attempt_demo_001',
      notes: 'Debit attempt authorized using stored source token.',
    },
    {
      id: 'transfer-1',
      type: 'Transfer',
      status: 'Processing',
      atUtc: '2026-07-28T01:08:00Z',
      amount: 8075,
      currency: 'AUD',
      reference: 'transfer_demo_001',
      notes: 'Contractor transfer initiated after platform fee allocation.',
    },
    {
      id: 'transfer-2',
      type: 'Transfer',
      status: 'Queued',
      atUtc: '2026-07-28T01:09:00Z',
      amount: 1425,
      currency: 'AUD',
      reference: 'transfer_demo_002',
      notes: 'Platform fee transfer queued for settlement ledger posting.',
    },
  ],
};
