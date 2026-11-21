export type User = { id: string; name: string; email: string; role: 'Admin' | 'Manager' | 'Member' };
export type AuthResponse = { accessToken: string; expiresAt: string; user: User };
export type Project = {
  id: string; name: string; description: string; status: string; startDate?: string | null;
  dueDate?: string | null; createdAt: string; memberCount: number; taskCount: number; completedTaskCount: number; canManage: boolean;
};
export type WorkTask = {
  id: string; projectId: string; title: string; description: string; status: string; priority: string;
  assigneeId?: string | null; assigneeName?: string | null; createdById: string; dueDate?: string | null;
  createdAt: string; updatedAt: string; commentCount: number;
};
export type ProjectMember = { userId: string; name: string; email: string; role: 'Manager' | 'Member'; joinedAt: string };
export type Comment = { id: string; taskId: string; authorId: string; authorName: string; body: string; createdAt: string };
export type BreakdownItem = { name: string; count: number };
export type DashboardSummary = {
  projectCount: number; openTaskCount: number; completedTaskCount: number; overdueTaskCount: number;
  statusBreakdown: BreakdownItem[]; priorityBreakdown: BreakdownItem[]; recentTasks: WorkTask[];
};
