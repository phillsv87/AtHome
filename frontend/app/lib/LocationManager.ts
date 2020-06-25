import HsApp from "./HsApp";
import { Location, ApiInfo, NotificationDevice, NotificationType } from "./types";
import Log from "../CommonJs/Log";
import { aryRemoveItem } from "../CommonJs/commonUtils";

const storeKey="LocationManager";

export default class LocationManager
{

    private readonly app:HsApp;

    private _locations:ReadonlyArray<Location>=[];
    public get locations(){return this._locations}

    constructor(app:HsApp)
    {
        this.app=app;
    }

    public async initAsync()
    {
        try{

            this._locations=await this.app.store.loadOrDefaultAsync<Location[]>(storeKey,[]);

        }catch(ex){
            Log.error('Unable to load locations',ex);
        }
    }

    public async setLocationAsync(id:string|null|undefined, name:string,apiBaseUrl:string,token:string,receiveNotifications:boolean):Promise<Location>
    {

        if(!apiBaseUrl.endsWith('/')){
            apiBaseUrl+='/';
        }

        if(!id){
            try{
                const info=await this.app.http.getAsync<ApiInfo>(apiBaseUrl+'api/Info');
                id=info.Id;
            }catch{
                throw new Error('Invalid URL')
            }

            try{
                //test token
                await this.app.http.getAsync(`${apiBaseUrl}api/Stream`,{clientToken:token});
            }catch{
                throw new Error('Invalid Token');
            }
        }else{

        }

        const locations=[...this._locations];

        const location:Location={
            Id:id,
            Name:name,
            ApiBaseUrl:apiBaseUrl,
            Token:token,
            ReceiveNotifications:receiveNotifications
        }
        const i=locations.findIndex(l=>l.Id===id);
        if(i===-1){
            locations.push(location);
        }else{
            locations[i]=location;
        }


        this._locations=locations;

        await this.app.store.saveAsync(storeKey,this._locations);

        try{
            await this.updateNotificationsSettings(id);
        }catch(ex){
            Log.error('Unable to update notifications settings for location',ex);
        }

        return location;

    }

    public async updateNotificationsSettings(id:string):Promise<boolean|undefined>
    {

        const location=this.app.locations.getLocation(id);
        if(!location){
            return undefined;
        }

        const devicePushId=this.app.device.getState().devicePushId;

        if(!devicePushId){
            return undefined;
        }

        const nd:NotificationDevice={
            Id:devicePushId,
            Type:NotificationType.APN
        }

        return location.ReceiveNotifications?
            await this.app.http.postAsync<boolean>(
                `${location.ApiBaseUrl}api/Notifications/Device?clientToken=${location.Token}`,nd):
            await this.app.http.deleteAsync<boolean>(
                `${location.ApiBaseUrl}api/Notifications/Device/${devicePushId}?clientToken=${location.Token}`);
    }

    public async deleteLocationAsync(id:string):Promise<boolean>
    {
        const location=this.getLocation(id);
        if(!location){
            return false;
        }

        location.ReceiveNotifications=false;
        try{
            await this.updateNotificationsSettings(id);
        }catch(ex){
            Log.error('Unable to update notifications settings for location',ex);
        }

        const nl=[...this._locations];
        aryRemoveItem(nl,location);
        this._locations=nl;

        await this.app.store.saveAsync(storeKey,this._locations);


        return true;
    }

    public getLocation(id:string|null|undefined):Location|null
    {
        return this._locations.find(l=>l.Id===id)||null;
    }
}