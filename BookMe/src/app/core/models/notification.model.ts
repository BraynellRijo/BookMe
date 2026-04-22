export interface NotificationDTO {
  id: string;
  userId: string;
  title: string;
  message: string;
  isRead: boolean;
  createdAt: string;
}

export interface NotificationCreationDTO {
  userId: string;
  title: string;
  message: string;
}
