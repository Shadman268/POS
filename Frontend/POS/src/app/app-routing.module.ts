import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { PosUiComponent } from './shared/components/pos-ui/pos-ui.component';

const routes: Routes = [
  {path:'', component: PosUiComponent}
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
