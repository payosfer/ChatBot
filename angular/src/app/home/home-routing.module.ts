import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { HomeComponent } from './home.component';



const routes: Routes = [
  { path: '', component: HomeComponent },
  { path: 'chat',
    loadComponent: () =>import('../chat/chat.component').then((m) => m.ChatComponent), // Standalone bileşen yüklemesi
  },

];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class HomeRoutingModule {}
