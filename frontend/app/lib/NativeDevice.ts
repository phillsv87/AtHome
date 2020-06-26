import HsApp from "./HsApp";
import { NativeModules } from "react-native";
import util from "../CommonJs/util";
import EventEmitterEx from "../CommonJs/EventEmitterEx-rn";
import { LocationAlert } from "./types";
import Log from "../CommonJs/Log";

const Delegate = NativeModules.HomeSecureDelegate;

export const StateChangeEvent=Symbol();

const appActionSplit='||||';

const storeKey='NativeDevice';


export interface NativeDeviceState
{
    readonly devicePushId:string|null;
    readonly appAction:string|null;
}


function createDefaultNativeState():NativeDeviceState
{
    return {
        devicePushId:null,
        appAction:null
    };
}

export class NativeDevice extends EventEmitterEx
{

    private readonly app:HsApp;

    private running:boolean=false;
    private loopStarted:boolean=false;
    private currentState:NativeDeviceState=createDefaultNativeState();

    constructor(app:HsApp)
    {
        super();
        this.app=app;
    }

    public dispose()
    {
        this.running=false;
    }

    public async initAsync ()
    {
        const state=await this.app.store.loadAsync<NativeDeviceState>(storeKey);
        if(state){
            this.currentState=state;
        }
        this.running=true;
        this.updateLoop();
    }

    private updateLoop(){

        if(this.loopStarted){
            return;
        }

        this.loopStarted=true;
        let uid:number=-1;
    
        const doUpdate=()=>{
            Delegate.getStatusWithUpdates(uid,(error:any,updateId:number,state:NativeDeviceState|null)=>{
                if(error){
                    console.log('getStatusWithUpdates error',error);
                    if(this.running){
                        setTimeout(doUpdate,2000);
                    }
                    return;
                }
                uid=updateId;
                if(!state){
                    state=createDefaultNativeState();
                }
                if(!util.areEqualShallow(state,this.currentState)){
                    this.onStateChange(state);
                }
                if(!this.running){
                    return;
                }
                doUpdate();
            });
            
        }
    
        doUpdate();
    }

    private async onStateChange(state:NativeDeviceState)
    {

        let save=false;
        

        if(state.devicePushId!==this.currentState.devicePushId){
            save=true;
        }

        if(state.appAction && state.appAction!==this.currentState.appAction){
            let i=state.appAction.indexOf(appActionSplit);
            const actionBody=state.appAction.substr(i+appActionSplit.length);
            i=actionBody.indexOf(':');
            let actionType:string;
            let actionPayload:string;
            if(i===-1){
                actionType=actionBody;
                actionPayload='';
            }else{
                actionType=actionBody.substr(0,i);
                actionPayload=actionBody.substr(i+1);;
            }

            switch(actionType){
                case 'LocationAlert':
                    if(actionPayload){
                        const at:LocationAlert=JSON.parse(actionPayload);
                        const location=this.app.locations.getLocation(at.Id);
                        if(location){
                            if(at.Stream){
                                this.app.history.push(`/location/${at.Id}/stream/${at.Stream.Id}`);
                                Log.infoUI(`Alert from ${location.Name} - ${at.Stream.Name}`);
                            }else{
                                this.app.history.push(`/location/${at.Id}`);
                                Log.infoUI(`Alert from ${location.Name}`);
                            }
                        }
                    }
                    break;
            }
            save=true;
        }

        this.currentState=state;

        if(save){
            this.app.store.saveAsync(storeKey,this.currentState);
        }

        this.emit(StateChangeEvent);
    }

    public requestNotificationsPermission()
    {
        Delegate.requestNotificationsPermission();
    }

    public getState():NativeDeviceState
    {
        return {...this.currentState}
    }
}