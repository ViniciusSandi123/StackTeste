import { TaskStatus } from './TaskStatus.enum';

export interface TaskItem {
  id: number;
  leadId: number;
  title: string;
  dueDate?: string;
  status: TaskStatus;
  createdAt: string;
  updatedAt: string;
}
