import { NgModule } from '@angular/core';
import { PageModule } from '@abp/ng.components/page';
import { SharedModule } from '../shared/shared.module';
import { HomeRoutingModule } from './home-routing.module';
import { HomeComponent } from './home.component';
import { ChatComponent } from '../chat/chat.component'; // ✅ Chat bileşenini import et
import { FormsModule } from '@angular/forms'; // ✅ ngModel için FormsModule

@NgModule({
  declarations: [
    HomeComponent,
  ],
  imports: [
    SharedModule, 
    HomeRoutingModule, 
    PageModule,
    FormsModule,
    ChatComponent,  // Standalone olarak tanımlandı o yüzden imports a ekledik
  ],
})
export class HomeModule {}



