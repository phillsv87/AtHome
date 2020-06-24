import React, { useState, useEffect } from 'react';
import { View, StyleSheet } from 'react-native';
import { useApp } from '../lib/hooks';
import { StreamSession } from '../lib/types';
import Log from '../CommonJs/Log';
import { delayAsync } from '../CommonJs/utilTs';
import Busy from './Busy';

// @ts-ignore
import Video from 'react-native-video';

interface StreamProps
{
    streamId?:number;
}

export default function Stream({
    streamId
}:StreamProps){

    const {config,http,clientToken}=useApp();

    const [session,setSession]=useState<null|StreamSession>(null);

    useEffect(()=>{

        if(!clientToken || streamId===undefined){
            return;
        }
        let m=true;
        let session:StreamSession|null=null;
        const run=async ()=>{
            try{
                session=await http.getAsync<StreamSession>(`api/Stream/${streamId}/Open`,{clientToken});
                if(!m){return;}
                setSession(session);
            }catch(ex){
                Log.error('Unable to start streaming session');
                return;
            }

            try{
                while(m){
                    await delayAsync(session.TTLSeconds*1000*0.75);
                    if(!m){break;}
                    await http.getAsync(`api/Stream/${streamId}/Extend/${session.Id}`,{clientToken,sessionToken:session.Token})
                }
            }catch(ex){
                Log.error('Unable to extend streaming session');
                return;
            }
        }
        run();

        const close=async ()=>{
            try{
                if(session){
                    await http.getAsync(`api/Stream/${streamId}/Close/${session.Id}`,{clientToken,sessionToken:session.Token});
                }
            }catch{}
        }

        return ()=>{m=false;close()}


    },[streamId,config,http,clientToken]);

    return (
        <View style={styles.root}>
            {session?
                <Video
                    style={styles.video}
                    source={{uri:config.ApiBaseUrl+session.Uri}}/>
                :
                <Busy/>
            }
        </View>
    )

}

const styles=StyleSheet.create({
    root:{
        flex:1
    },
    video:{
       flex:1
    }
});
