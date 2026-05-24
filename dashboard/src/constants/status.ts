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
} as const

export const QualityStatus = {
  Passed: 'passed',
  Rework: 'rework',
  Scrapped: 'scrapped',
} as const

export const Severity = {
  None: 'none',
  Minor: 'minor',
  Major: 'major',
  Critical: 'critical',
} as const