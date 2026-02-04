import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { PosUiComponent } from './shared/components/pos-ui/pos-ui.component';
import { LoginComponent } from './shared/components/login/login.component';
import { RegisterComponent } from './shared/components/register/register.component';
import { AuthGuard } from './core/guards/auth.guard';

const routes: Routes = [
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  {
    path: 'pos',
    component: PosUiComponent,
    canActivate: [AuthGuard]
  },
  {
    path: '',
    redirectTo: 'pos',
    pathMatch: 'full'
  },
  { path: '**', redirectTo: 'pos' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
