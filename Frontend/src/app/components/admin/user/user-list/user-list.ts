import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Invitation, Permission, User } from '../../../../models/user.model';
import { AdminService } from '../../../../services/admin.service';

@Component({
  selector: 'app-user-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './user-list.html',
  styleUrl: './user-list.css',
})
export class UserList implements OnInit {
  activeTab = 'users';
  users: User[] = [];
  invitations: Invitation[] = [];
  permissions: Permission[] = [];
  searchTerm = '';

  showInviteModal = false;
  inviteEmail = '';
  inviteRole = 'Operator';
  inviteLink = '';

  constructor(
    private adminService: AdminService,
    private cdr: ChangeDetectorRef
  ) { }
  ngOnInit(): void {
    this.loadUsers();
    this.loadInvitations();
    this.loadPermissions();
  }
  loadUsers(): void {
    this.adminService.getUsers().subscribe({
      next: (data) => {
        this.users = data,
          this.cdr.detectChanges();
      },
      error: (err) => console.error(err)
    });
  }

  loadInvitations(): void {
    this.adminService.getInvitations().subscribe({
      next: (data) => {
        this.invitations = data,
          this.cdr.detectChanges();
      },
      error: (err) => console.error(err)
    });
  }

  loadPermissions(): void {
    this.adminService.getPermissions().subscribe({
      next: (data) => {
        this.permissions = data,
          this.cdr.detectChanges();
      },
      error: (err) => console.error(err)
    });
  }

  get filteredUsers(): User[] {
    return this.users.filter(u =>
      u.fullName.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
      u.email.toLowerCase().includes(this.searchTerm.toLowerCase())
    );
  }
  get permissionCategories(): { name: string; permissions: Permission[] }[] {
    const categories = new Map<string, Permission[]>();
    this.permissions.forEach(p => {
      if (!categories.has(p.category)) categories.set(p.category, []);
      categories.get(p.category)!.push(p);
    });
    return Array.from(categories.entries()).map(([name, permissions]) => ({ name, permissions }));
  }
  getUserInitials(user: User): string {
    return `${user.prenom?.charAt(0)}${user.nom?.charAt(0)}`;
  }

  updateRole(user: User): void {
    this.adminService.updateUserRole(user.id, user.role).subscribe({
      next: () => this.loadUsers(),
      error: (err) => console.error(err)
    });
  }

  toggleStatus(user: User): void {
    if (user.isActive) {
      this.adminService.deactivateUser(user.id).subscribe(() => this.loadUsers());
    } else {
      this.adminService.activateUser(user.id).subscribe(() => this.loadUsers());
    }
  }

  deleteUser(user: User): void {
    if (confirm(`Supprimer l'utilisateur "${user.fullName}" ?`)) {
      this.adminService.deleteUser(user.id).subscribe(() => this.loadUsers());
    }
  }

  sendInvitation(): void {
    if (!this.inviteEmail) return;

    this.adminService.createInvitation(this.inviteEmail, this.inviteRole).subscribe({
      next: (data) => {
        const baseUrl = window.location.origin;
        this.inviteLink = `${baseUrl}/accept-invitation?token=${data.token}`;
        this.loadInvitations();
      },
      error: (err) => console.error(err)
    });
  }

  copyInvitationLink(invitation: Invitation): void {
    const baseUrl = window.location.origin;
    const link = `${baseUrl}/accept-invitation?token=${invitation.token}`;
    this.copyToClipboard(link);
  }

  copyToClipboard(text: string): void {
    navigator.clipboard.writeText(text);
    alert('Lien copié dans le presse-papier !');
  }

  cancelInvitation(id: number): void {
    if (confirm('Annuler cette invitation ?')) {
      this.adminService.cancelInvitation(id).subscribe(() => this.loadInvitations());
    }
  }

  closeInviteModal(): void {
    this.showInviteModal = false;
    this.inviteEmail = '';
    this.inviteRole = 'Operator';
    this.inviteLink = '';
  }

  closeModalOnOverlay(event: MouseEvent): void {
    if ((event.target as HTMLElement).classList.contains('modal')) {
      this.closeInviteModal();
    }
  }
}
