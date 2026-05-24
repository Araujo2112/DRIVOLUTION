export const EntityStatus = {
  Active: 'active',
  Inactive: 'inactive',
  Pending: 'pending',
  InProgress: 'in_progress',
  Completed: 'completed',
  Cancelled: 'cancelled',
  Functional: 'functional',
  Maintenance: 'maintenance',
  Broken: 'broken',
  Unknown: 'unknown',
} as const

export const QualityStatus = {
  Passed: 'passed',
  Completed: 'completed',
  InProgress: 'in_progress',
  Rework: 'failed_rework',
  Scrapped: 'failed_scrapped',
} as const

export const Severity = {
  None: 'none',
  Minor: 'minor',
  Major: 'major',
  Critical: 'critical',
} as const