import {AfterViewInit, Component, OnInit, ViewChild} from '@angular/core';
import {DashboardService} from "../services/dashboard/dashboard.service";
import {NavigationEnd, Router} from "@angular/router";
import {StorageService} from '../services/storage/storage.service';
import {StorageKey} from '../consts/storage-key';
import {BreakpointObserver} from '@angular/cdk/layout';
import {MatSidenav} from '@angular/material/sidenav';

@Component({
  selector: 'app-root',
  templateUrl: './root.component.html',
  styleUrls: ['./root.component.scss']
})
export class RootComponent implements OnInit, AfterViewInit {

  sessionId: string | undefined;
  menus: any = [];

  @ViewChild('drawer') drawer!: MatSidenav;

  constructor(
    private router: Router,
    private storageService: StorageService,
    private dashboardService: DashboardService,
    private breakpointObserver: BreakpointObserver
  ) {
    this.buildMenu();
  }

  ngAfterViewInit(): void {
    this.loadDrawerState();
    this.enableResponsiveMode();
  }

  ngOnInit(): void {
    this.router.events.subscribe((val) => {
      if (val instanceof NavigationEnd) {
        this.buildMenu();
      }
    })

  }

  onLogout() {
    this.dashboardService.logoutSession();
    this.router.navigate(['/']).then(r => {
      console.debug('after-logout', r);
      window.location.reload();
    });
  }

  buildMenu() {
    this.sessionId = this.storageService.get('session_id');

    this.menus = [
      {
        title: 'Mirror User',
        url: '/mirror-user/management',
        minimumRole: 1
      },
      {
        title: 'Anti-Spam',
        url: '/antispam/fed-ban-management',
        minimumRole: 1
      },
      {
        title: 'Notes',
        url: '/notes',
        minimumRole: 1
      },
      {
        title: 'Angular',
        url: '/angular',
        minimumRole: 9
      }
    ];
  }

  toggleDrawer() {
    this.storageService.set(StorageKey.DRAWER_STATE, this.drawer.opened ? 'false' : 'true');
    this.drawer.toggle().then(r => console.debug('drawer status:', r));
  }

  loadDrawerState() {
    const drawer = this.storageService.get(StorageKey.DRAWER_STATE);
    if (drawer == 'true') {
      this.drawer.open().then(r => console.debug('drawer status:', r));
    } else {
      this.drawer.close().then(r => console.debug('drawer status:', r));
    }
  }

  enableResponsiveMode() {
    this.breakpointObserver.observe(['(max-width: 800px)']).subscribe(res => {
      if (res.matches) {
        this.drawer.mode = 'over';
        this.drawer.close().then(r => console.debug('drawer status:', r));
      } else {
        this.drawer.mode = 'side';
        this.drawer.open().then(r => console.debug('drawer status:', r));
      }
    });
  }
}