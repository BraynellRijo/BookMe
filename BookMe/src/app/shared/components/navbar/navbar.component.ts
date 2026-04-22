import { Component, inject, signal, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { animate, style, transition, trigger } from '@angular/animations';
import { TokenService } from '../../../core/services/token.service';
import { SearchExplore } from '../search-explore/search-explore';

import { NotificationsComponent } from '../notifications/notifications';

@Component({
    selector: 'app-navbar',
    standalone: true,
    imports: [CommonModule, RouterLink, RouterLinkActive, SearchExplore, NotificationsComponent],
    schemas: [CUSTOM_ELEMENTS_SCHEMA],
    template: `
    <!-- TopAppBar -->
    <header
        class="fixed top-0 w-full z-50 glass-nav shadow-[0_20px_40px_rgba(27,28,26,0.02)] transition-colors duration-300">
        <div class="flex justify-between items-center px-6 py-4 max-w-7xl mx-auto">
            <!-- Left: Logo -->
            <div class="flex-1">
                <div class="text-2xl font-bold tracking-tighter text-primary font-headline inline-block cursor-pointer"
                    routerLink="/">BookMe
                </div>
            </div>

            <!-- Center: Integrated Search Bar (Desktop) -->
            <div class="flex-initial">
                <div class="hidden md:flex items-center justify-between bg-surface-container-high rounded-full pl-6 pr-2 py-1.5 w-[380px] lg:w-[460px] transition-all hover:bg-surface-container-highest cursor-pointer border border-outline-variant/20 group"
                     (click)="showSearchOverlay()">
                    <div class="text-on-surface-variant/70 font-body text-[13px] tracking-tight">
                        Busca tu próximo refugio...
                    </div>
                    <div class="px-5 py-2 rounded-full bg-primary text-white font-headline font-bold text-[11px] uppercase tracking-widest flex items-center gap-2 hover:bg-secondary transition-all shadow-md active:scale-95 group-hover:shadow-primary/20">
                        <span class="material-symbols-outlined text-[18px]">search</span>
                        Explorar
                    </div>
                </div>
            </div>

            <!-- Right: Actions & User Menu -->
            <div class="flex-1 flex justify-end items-center space-x-6">
                <!-- Host options -->
                <a *ngIf="isGuest() && !isHost()" class="hidden lg:block text-sm font-medium text-on-surface hover:text-primary transition-colors cursor-pointer"
                    routerLink="/host">Convertirme en anfitrión</a>
                <a *ngIf="isHost()" class="hidden lg:block text-sm font-medium text-on-surface hover:text-primary transition-colors cursor-pointer"
                    routerLink="/host/dashboard">Gestionar mis propiedades</a>

                <!-- Notifications -->
                <app-notifications *ngIf="isAuthenticated()"></app-notifications>

                <!-- User Dropdown container -->
                <div class="relative">
                    <button
                        class="text-primary hover:bg-surface-container-low p-2 rounded-full transition-colors flex items-center gap-2 border border-outline-variant/30 hover:shadow-md"
                        (click)="toggleDropdown($event)">
                        <span class="material-symbols-outlined text-2xl" *ngIf="!isAuthenticated(); else loggedInAvatarMenu">account_circle</span>

                        <ng-template #loggedInAvatarMenu>
                            <div
                                class="w-7 h-7 rounded-full overflow-hidden bg-surface-container flex items-center justify-center">
                                <img alt="User profile" class="w-full h-full object-cover"
                                    src="https://lh3.googleusercontent.com/aida-public/AB6AXuDNARpHn3julFvc4OjCRwfWM5CE6FXOCJ-Kx0SyMk1qzTuP4DTENU1uGo2-zJwX9nIB1Qpid7gZwER0MIFtDcl9Cn1Tf6gyu3_gIapb9XBqb2GqMZLRkxwVKNLmwR3_EqytMF_XiGNmasdPu1CJUNVaM3ChQzOsZ_AdvN5sT0_logz3VhoA8t-E82w-aG7D-D8ZHsyctPcbblh69klzIvYxb2ZEzQV5pGzeYkeP-Jd5A0qKI0ESevA4rHjtEPWzTGZvw19OfAivxiY1" />
                            </div>
                        </ng-template>
                        <span class="material-symbols-outlined text-xl hidden md:block">menu</span>
                    </button>

                    <!-- Dropdown Body -->
                    <div *ngIf="isDropdownOpen()" [@slideFade]
                        class="absolute right-0 mt-3 w-64 premium-dropdown rounded-3xl py-2 z-50 overflow-hidden"
                        (click)="$event.stopPropagation()">
                        <ng-container *ngIf="isAuthenticated(); else loggedOutMenu">
                            <div class="px-5 py-4 border-b border-outline/10 mb-1 bg-primary/[0.03]">
                                <p class="text-[9px] font-bold text-primary uppercase tracking-[0.2em] mb-1">
                                    Mi Cuenta</p>
                                <p class="text-sm font-bold truncate text-on-surface">
                                    {{ currentUser()?.firstName }} {{ currentUser()?.lastName }}
                                </p>
                                <p class="text-[11px] text-on-surface-variant truncate">
                                    {{ currentUser()?.email }}
                                </p>
                            </div>
                            <a class="dropdown-item" routerLink="/profile" (click)="closeDropdown()">
                                <span class="material-symbols-outlined">person</span> Perfil
                            </a>
                            <a class="dropdown-item" routerLink="/reservations" (click)="closeDropdown()">
                                <span class="material-symbols-outlined">work</span> Mis Viajes
                            </a>
                            <hr class="my-1 border-outline/10">
                            <a *ngIf="isGuest() && !isHost()" class="dropdown-item" routerLink="/host" (click)="closeDropdown()">
                                <span class="material-symbols-outlined">home</span> Convertirme en anfitrión
                            </a>
                            <a *ngIf="isHost()" class="dropdown-item" routerLink="/host/dashboard" (click)="closeDropdown()">
                                <span class="material-symbols-outlined">dashboard</span> Gestionar mis propiedades
                            </a>
                            <hr class="my-1 border-outline/10">
                            <a class="dropdown-item text-red-600 dark:text-red-400 hover:bg-red-50 dark:hover:bg-red-900/20 cursor-pointer" (click)="logout()">
                                <span class="material-symbols-outlined">logout</span> Cerrar sesión
                            </a>
                        </ng-container>
                        <ng-template #loggedOutMenu>
                            <a class="dropdown-item font-bold text-on-surface" routerLink="/auth/login" (click)="closeDropdown()">
                                <span class="material-symbols-outlined">login</span> Iniciar sesión
                            </a>
                            <a class="dropdown-item" routerLink="/auth/register" (click)="closeDropdown()">
                                <span class="material-symbols-outlined">person_add</span> Regístrate
                            </a>
                            <hr class="my-2 border-outline/10">
                            <a *ngIf="isGuest() && !isHost()" class="dropdown-item" routerLink="/host" (click)="closeDropdown()">
                                <span class="material-symbols-outlined">home</span> Convertirme en anfitrión
                            </a>
                            <a *ngIf="isHost()" class="dropdown-item" routerLink="/host/dashboard" (click)="closeDropdown()">
                                <span class="material-symbols-outlined">dashboard</span> Gestionar mis propiedades
                            </a>
                            <a class="dropdown-item" href="#">
                                <span class="material-symbols-outlined">help</span> Centro de ayuda
                            </a>
                        </ng-template>
                    </div>
                </div>
            </div>
        </div>
    </header>

    <!-- BottomNavBar (Mobile Floating) -->
    <nav
        class="md:hidden fixed bottom-6 left-1/2 -translate-x-1/2 w-[90%] max-w-md rounded-full px-6 py-3 bg-white/90 dark:bg-stone-900/80 backdrop-blur-xl shadow-ambient z-50 flex justify-around items-center transition-all duration-300">
        <button class="flex flex-col items-center justify-center text-primary dark:text-white scale-110" routerLink="/" routerLinkActive="text-primary" [routerLinkActiveOptions]="{exact: true}">
            <span class="material-symbols-outlined text-xl">search</span>
            <span class="font-label text-[11px] uppercase tracking-widest font-semibold mt-1">Explore</span>
        </button>
        <button
            class="flex flex-col items-center justify-center text-stone-400 dark:text-stone-500 hover:text-secondary transition-all scale-90 active:scale-100 duration-200">
            <span class="material-symbols-outlined text-xl">favorite_border</span>
            <span class="font-label text-[11px] uppercase tracking-widest font-semibold mt-1">Saved</span>
        </button>
        <button
            class="flex flex-col items-center justify-center text-stone-400 dark:text-stone-500 hover:text-secondary transition-all scale-90 active:scale-100 duration-200"
            [class.hidden]="!isAuthenticated()" routerLink="/reservations" routerLinkActive="text-primary">
            <span class="material-symbols-outlined text-xl">public</span>
            <span class="font-label text-[11px] uppercase tracking-widest font-semibold mt-1">Trips</span>
        </button>
        <button
            class="flex flex-col items-center justify-center text-stone-400 dark:text-stone-500 hover:text-secondary transition-all scale-90 active:scale-100 duration-200"
            [class.hidden]="!isAuthenticated()" routerLink="/inbox" routerLinkActive="text-primary">
            <span class="material-symbols-outlined text-xl">mail</span>
            <span class="font-label text-[11px] uppercase tracking-widest font-semibold mt-1">Inbox</span>
        </button>
        <button
            class="flex flex-col items-center justify-center text-stone-400 dark:text-stone-500 hover:text-secondary transition-all scale-90 active:scale-100 duration-200"
            routerLink="/auth/login" *ngIf="!isAuthenticated()">
            <span class="material-symbols-outlined text-xl">login</span>
            <span class="font-label text-[11px] uppercase tracking-widest font-semibold mt-1">Login</span>
        </button>
    </nav>
    
    <!-- Fullscreen Search Overlay -->
    @if(isSearchActive) {
      <app-search-explore (closeDialog)="hideSearchOverlay()"></app-search-explore>
    }
  `,
    animations: [
        trigger('slideFade', [
            transition(':enter', [
                style({ opacity: 0, transform: 'translateY(-10px)' }),
                animate('200ms ease-out', style({ opacity: 1, transform: 'translateY(0)' })),
            ]),
            transition(':leave', [
                animate('150ms ease-in', style({ opacity: 0, transform: 'translateY(-10px)' })),
            ]),
        ]),
    ],
})
export class NavbarComponent {
    private tokenService = inject(TokenService);
    private router = inject(Router);

    isDropdownOpen = signal(false);
    isSearchActive = false;
    isAuthenticated = this.tokenService.isAuthenticated;
    isHost = this.tokenService.isHost;
    isGuest = this.tokenService.isGuest;
    currentUser = this.tokenService.currentUser;

    constructor() {
        // Close dropdown on click elsewhere
        if (typeof document !== 'undefined') {
            document.addEventListener('click', () => this.closeDropdown());
        }
    }

    toggleDropdown(event: Event) {
        event.stopPropagation();
        this.isDropdownOpen.update(val => !val);
    }

    closeDropdown() {
        this.isDropdownOpen.set(false);
    }

    logout() {
        this.tokenService.clearTokens();
        this.closeDropdown();
        this.router.navigate(['/']);
    }

    showSearchOverlay() {
        this.isSearchActive = true;
        if (typeof document !== 'undefined') {
            document.body.style.overflow = 'hidden';
        }
    }

    hideSearchOverlay() {
        this.isSearchActive = false;
        if (typeof document !== 'undefined') {
            document.body.style.overflow = 'auto';
        }
    }
}
