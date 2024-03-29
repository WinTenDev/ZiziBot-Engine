import {NgModule} from '@angular/core';
import {RouterModule, Routes} from '@angular/router';
import {HomeComponent} from "./home/home.component";
import {WelcomeComponent} from "./welcome/welcome.component";
import {AfterTelegramLoginComponent} from "./after-telegram-login/after-telegram-login.component";

const routes: Routes = [
  {path: '', component: HomeComponent},
  {path: 'angular', component: WelcomeComponent},
  {path: 'after-telegram-login', component: AfterTelegramLoginComponent},
  {
    path: 'mirror-user',
    loadChildren: () => import('./modules/mirror-user/mirror-user.module').then(m => m.MirrorUserModule)
  },
  {
    path: 'antispam',
    loadChildren: () => import('./modules/antispam/antispam.module').then(m => m.AntispamModule)
  },
  {
    path: 'notes',
    loadChildren: () => import('./modules/notes/notes.module').then(m => m.NotesModule)
  },
  {
    path: 'group',
    loadChildren: () => import('./modules/group/group.module').then(m => m.GroupModule)
  },
  {
    path: 'session',
    loadChildren: () => import('./modules/session/session.module').then(m => m.SessionModule)
  }
];

@NgModule({
  imports: [
    RouterModule.forRoot(routes)
  ],
  exports: [RouterModule]
})
export class AppRoutingModule {
}